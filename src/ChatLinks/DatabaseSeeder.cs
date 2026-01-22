using System.Text.Json;

using GuildWars2;
using GuildWars2.Collections;
using GuildWars2.Hero.Achievements;
using GuildWars2.Hero.Achievements.Categories;
using GuildWars2.Hero.Achievements.Groups;
using GuildWars2.Hero.Crafting.Recipes;
using GuildWars2.Hero.Equipment.Dyes;
using GuildWars2.Hero.Equipment.Finishers;
using GuildWars2.Hero.Equipment.Gliders;
using GuildWars2.Hero.Equipment.JadeBots;
using GuildWars2.Hero.Equipment.MailCarriers;
using GuildWars2.Hero.Equipment.Novelties;
using GuildWars2.Hero.Equipment.Outfits;
using GuildWars2.Hero.Equipment.Wardrobe;
using GuildWars2.Items;
using GuildWars2.Pvp.MistChampions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SL.ChatLinks.StaticFiles;
using SL.ChatLinks.Storage;
using SL.ChatLinks.Storage.Metadata;

using Language = GuildWars2.Language;

namespace SL.ChatLinks;

public sealed class DatabaseSeeder : IDisposable
{
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly IOptions<DatabaseOptions> _options;
    private readonly IDbContextFactory _contextFactory;
    private readonly IEventAggregator _eventAggregator;
    private readonly Gw2Client _gw2Client;
    private readonly StaticDataClient _staticDataClient;
    private readonly ILocale _locale;
    private readonly SemaphoreSlim _syncSemaphore = new(1, 1);
    private Task? _currentSync;

    public DatabaseSeeder(ILogger<DatabaseSeeder> logger,
        IOptions<DatabaseOptions> options,
        IDbContextFactory contextFactory,
        IEventAggregator eventAggregator,
        Gw2Client gw2Client,
        StaticDataClient staticDataClient,
        ILocale locale
    )
    {
        ThrowHelper.ThrowIfNull(eventAggregator);
        _logger = logger;
        _options = options;
        _contextFactory = contextFactory;
        _eventAggregator = eventAggregator;
        _gw2Client = gw2Client;
        _staticDataClient = staticDataClient;
        _locale = locale;
        eventAggregator.Subscribe<LocaleChanged>(OnLocaleChanged);
        eventAggregator.Subscribe<HourStarted>(OnHourStarted);
    }

    private async Task OnLocaleChanged(LocaleChanged args)
    {
        await Task.Run(async () =>
        {
            await Migrate(args.Language).ConfigureAwait(false);
            await Sync(args.Language, CancellationToken.None).ConfigureAwait(false);
            await Vacuum(args.Language, CancellationToken.None).ConfigureAwait(false);
            await Optimize(args.Language, CancellationToken.None).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private async Task OnHourStarted(HourStarted args)
    {
        await Task.Run(async () =>
        {
            await Migrate(_locale.Current).ConfigureAwait(false);
            await Sync(_locale.Current, CancellationToken.None).ConfigureAwait(false);
            await Vacuum(_locale.Current, CancellationToken.None).ConfigureAwait(false);
            await Optimize(_locale.Current, CancellationToken.None).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    private async ValueTask<DataManifest?> DataManifest()
    {
        try
        {
            string path = Path.Combine(_options.Value.Directory, "manifest.json");
            if (!File.Exists(path))
            {
                return null;
            }

            using FileStream stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<DataManifest>(stream).ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogError(reason, "Failed to read manifest.");
            return null;
        }
    }

    public async ValueTask SaveManifest(DataManifest manifest)
    {
        try
        {
            string path = Path.Combine(_options.Value.Directory, "manifest.json");
            using FileStream stream = File.OpenWrite(path);
            await JsonSerializer.SerializeAsync(stream, manifest).ConfigureAwait(false);
        }
        catch (Exception reason)
        {
            _logger.LogError(reason, "Failed to save updated manifest.");
        }
    }

    private bool IsEmpty(Database database)
    {
        string location = Path.Combine(_options.Value.Directory, database.Name);
        return new FileInfo(location) is { Exists: false } or { Length: 0 };
    }

    public async Task Migrate(Language language)
    {
        ThrowHelper.ThrowIfNull(language);
        DataManifest currentDataManifest = await DataManifest().ConfigureAwait(false) ?? new DataManifest
        {
            Version = 1,
            Databases = []
        };

        bool shouldDownload = !currentDataManifest.Databases.TryGetValue(language.Alpha2Code, out Database? currentDatabase)
            || currentDatabase.SchemaVersion != ChatLinksContext.SchemaVersion
            || IsEmpty(currentDatabase)
            || !await IsReady(currentDatabase).ConfigureAwait(false);

        if (shouldDownload)
        {
            Database? seedDatabase = await DownloadDatabase(language).ConfigureAwait(false);
            if (seedDatabase is not null)
            {
                currentDatabase = seedDatabase;
                currentDataManifest.Databases[language.Alpha2Code] = seedDatabase;

                await SaveManifest(currentDataManifest).ConfigureAwait(false);
            }
        }

        if (currentDatabase is null)
        {
            _logger.LogWarning("No usable database found for language {Language}.", language.Alpha2Code);
            return;
        }

        ChatLinksContext context = _contextFactory.CreateDbContext(currentDatabase.Name);
        if (await HasPendingMigrations(context).ConfigureAwait(false))
        {
            await using (context.ConfigureAwait(false))
            {
                await context.Database.MigrateAsync().ConfigureAwait(false);
            }

            currentDatabase.SchemaVersion = ChatLinksContext.SchemaVersion;
            await SaveManifest(currentDataManifest).ConfigureAwait(false);
            await _eventAggregator.PublishAsync(new DatabaseMigrated()).ConfigureAwait(false);
        }
    }

    private static async Task<bool> HasPendingMigrations(ChatLinksContext context)
    {
        IEnumerable<string> migrations = await context.Database.GetPendingMigrationsAsync().ConfigureAwait(false);
        return migrations.Any();
    }

    private async Task<bool> IsReady(Database database)
    {
        try
        {
            ChatLinksContext context = _contextFactory.CreateDbContext(database.Name);
            await using (context.ConfigureAwait(false))
            {
                return !await HasPendingMigrations(context).ConfigureAwait(false);
            }
        }
        catch (SqliteException)
        {
            return false;
        }
    }

    private async Task<Database?> DownloadDatabase(Language language)
    {
        SeedIndex seedDataManifest = await _staticDataClient.GetSeedIndex(CancellationToken.None).ConfigureAwait(false);
        SeedDatabase? seedDatabase = seedDataManifest.Databases
            .OrderByDescending(seed => seed.SchemaVersion)
            .FirstOrDefault(seed => seed.SchemaVersion <= ChatLinksContext.SchemaVersion
                && seed.Language == language.Alpha2Code);

        if (seedDatabase is null)
        {
            return null;
        }

        string destination = Path.Combine(_options.Value.Directory, seedDatabase.Name);
        await _staticDataClient.Download(seedDatabase, destination, CancellationToken.None).ConfigureAwait(false);

        return new Database
        {
            Name = seedDatabase.Name,
            SchemaVersion = seedDatabase.SchemaVersion
        };
    }

    public async Task Optimize(Language language, CancellationToken cancellationToken)
    {
        ChatLinksContext context = _contextFactory.CreateDbContext(language);
        await using (context.ConfigureAwait(false))
        {
            _ = await context.Database.ExecuteSqlRawAsync("PRAGMA optimize;", cancellationToken)
                    .ConfigureAwait(false);
        }
    }

    public async Task Vacuum(Language language, CancellationToken cancellationToken)
    {
        ChatLinksContext context = _contextFactory.CreateDbContext(language);
        await using (context.ConfigureAwait(false))
        {
            _ = await context.Database.ExecuteSqlRawAsync("VACUUM;", cancellationToken)
                    .ConfigureAwait(false);
        }
    }

    public bool IsSynchronizing => _currentSync is { IsCompleted: false };

    public async Task Sync(Language language, CancellationToken cancellationToken)
    {
        await _syncSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_currentSync is null || _currentSync.IsCompleted)
            {
                _currentSync = Task.Run(async () =>
                {
                    ChatLinksContext context = _contextFactory.CreateDbContext(language);
                    await using (context.ConfigureAwait(false))
                    {
                        context.ChangeTracker.AutoDetectChangesEnabled = false;
                        await Seed(context, language, cancellationToken).ConfigureAwait(false);
                    }
                }, cancellationToken);
            }
        }
        finally
        {
            _ = _syncSemaphore.Release();
        }

        await _currentSync.ConfigureAwait(false);
        await _eventAggregator.PublishAsync(new DatabaseSyncCompleted(), cancellationToken).ConfigureAwait(false);
    }

    public async Task SeedAll()
    {
        _ = Directory.CreateDirectory(_options.Value.Directory);
        DataManifest manifest = new()
        {
            Version = 1,
            Databases = []
        };

        foreach (Language language in (Language[])[Language.English, Language.German, Language.French, Language.Spanish])
        {
            Database database = new()
            {
                Name = _options.Value.DatabaseFileName(language),
                SchemaVersion = ChatLinksContext.SchemaVersion
            };

            ChatLinksContext context = _contextFactory.CreateDbContext(database.Name);
            await using (context.ConfigureAwait(false))
            {
                await context.Database.MigrateAsync().ConfigureAwait(false);
                await Seed(context, language, CancellationToken.None).ConfigureAwait(false);
                await Vacuum(language, CancellationToken.None).ConfigureAwait(false);
                await Optimize(language, CancellationToken.None).ConfigureAwait(false);
            }

            manifest.Databases[language.Alpha2Code] = database;
        }

        await SaveManifest(manifest).ConfigureAwait(false);
    }

    private async Task Seed(ChatLinksContext context, Language language, CancellationToken cancellationToken)
    {
        Dictionary<string, int> inserted = new()
        {
            ["items"] = await SeedItems(context, language, cancellationToken).ConfigureAwait(false),
            ["skins"] = await SeedSkins(context, language, cancellationToken).ConfigureAwait(false),
            ["recipes"] = await SeedRecipes(context, cancellationToken).ConfigureAwait(false),
            ["colors"] = await SeedColors(context, language, cancellationToken).ConfigureAwait(false),
            ["finishers"] = await SeedFinishers(context, language, cancellationToken).ConfigureAwait(false),
            ["gliders"] = await SeedGliders(context, language, cancellationToken).ConfigureAwait(false),
            ["jade_bots"] = await SeedJadeBots(context, language, cancellationToken).ConfigureAwait(false),
            ["mail_carriers"] = await SeedMailCarriers(context, language, cancellationToken).ConfigureAwait(false),
            ["miniatures"] = await SeedMiniatures(context, language, cancellationToken).ConfigureAwait(false),
            ["mist_champions"] = await SeedMistChampions(context, language, cancellationToken).ConfigureAwait(false),
            ["novelties"] = await SeedNovelties(context, language, cancellationToken).ConfigureAwait(false),
            ["outfits"] = await SeedOutfits(context, language, cancellationToken).ConfigureAwait(false),
            ["achievements"] = await SeedAchievements(context, language, cancellationToken).ConfigureAwait(false),
            ["achievement_categories"] = await SeedAchievementCategories(context, language, cancellationToken).ConfigureAwait(false),
            ["achievement_groups"] = await SeedAchievementGroups(context, language, cancellationToken).ConfigureAwait(false)
        };

        await _eventAggregator.PublishAsync(new DatabaseSeeded(language, inserted), cancellationToken).ConfigureAwait(false);
    }

    private async Task<int> SeedItems(ChatLinksContext context, Language language, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding items.");

        IImmutableValueSet<int> index = await _gw2Client.Items
            .GetItemsIndex(cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        _logger.LogDebug("Found {Count} items in the API.", index.Count);
        List<int> existing = await context.Items.Select(item => item.Id)
            .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        index = index.Except(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} items.", index.Count);
            Progress<BulkProgress> bulkProgress = new(report =>
            {
                _eventAggregator.Publish(new DatabaseSyncProgress("items", report));
            });

            int count = 0;
            await foreach (Item item in _gw2Client.Items
                               .GetItemsBulk(index,
                                   language,
                                   MissingMemberBehavior.Undefined,
                                   3,
                                   200,
                                   bulkProgress,
                                   cancellationToken)
                               .ValueOnly(cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                _ = context.Add(item);
                if (++count % 333 == 0)
                {
                    _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    DetachAllEntities(context);
                }
            }

            _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} items.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedSkins(ChatLinksContext context, Language language, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding skins.");

        IImmutableValueSet<int> index = await _gw2Client.Hero.Equipment.Wardrobe
            .GetSkinsIndex(cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        _logger.LogDebug("Found {Count} skins in the API.", index.Count);
        List<int> existing = await context.Skins.Select(skin => skin.Id)
            .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        index = index.Except(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} skins.", index.Count);
            Progress<BulkProgress> bulkProgress = new(report =>
            {
                _eventAggregator.Publish(new DatabaseSyncProgress("skins", report));
            });

            int count = 0;
            await foreach (EquipmentSkin skin in _gw2Client.Hero.Equipment.Wardrobe
                               .GetSkinsBulk(index,
                                   language,
                                   MissingMemberBehavior.Undefined,
                                   3,
                                   200,
                                   bulkProgress,
                                   cancellationToken)
                               .ValueOnly(cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                _ = context.Add(skin);
                if (++count % 333 == 0)
                {
                    _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    DetachAllEntities(context);
                }
            }

            _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} skins.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedColors(ChatLinksContext context, Language language, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding colors.");

        IImmutableValueSet<int> index = await _gw2Client.Hero.Equipment.Dyes
            .GetColorsIndex(cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        _logger.LogDebug("Found {Count} colors in the API.", index.Count);
        List<int> existing = await context.Colors.Select(color => color.Id)
            .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        index = index.Except(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} colors.", index.Count);

            // TODO: incremental query
            IImmutableValueSet<DyeColor> colors = await _gw2Client.Hero.Equipment.Dyes
                .GetColors(language, MissingMemberBehavior.Undefined, cancellationToken).ValueOnly().ConfigureAwait(false);

            await context.AddRangeAsync(colors.Where(color => index.Contains(color.Id)), cancellationToken).ConfigureAwait(false);
            _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} colors.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedRecipes(ChatLinksContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding recipes.");

        IImmutableValueSet<int> index = await _gw2Client.Hero.Crafting.Recipes
            .GetRecipesIndex(cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        _logger.LogDebug("Found {Count} recipes in the API.", index.Count);
        List<int> existing = await context.Recipes.Select(recipe => recipe.Id)
            .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        index = index.Except(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} recipes.", index.Count);
            Progress<BulkProgress> bulkProgress = new(report =>
            {
                _eventAggregator.Publish(new DatabaseSyncProgress("recipes", report));
            });

            int count = 0;
            await foreach (Recipe recipe in _gw2Client.Hero.Crafting.Recipes
                               .GetRecipesBulk(index,
                                   MissingMemberBehavior.Undefined,
                                   3,
                                   200,
                                   bulkProgress,
                                   cancellationToken)
                               .ValueOnly(cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                _ = context.Add(recipe);
                if (++count % 333 == 0)
                {
                    _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    DetachAllEntities(context);
                }
            }

            _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} recipes.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedFinishers(ChatLinksContext context, Language language, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding finishers.");

        IImmutableValueSet<int> index = await _gw2Client.Hero.Equipment.Finishers
            .GetFinishersIndex(cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        _logger.LogDebug("Found {Count} finishers in the API.", index.Count);
        List<int> existing = await context.Finishers.Select(finisher => finisher.Id)
            .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        index = index.Except(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} finishers.", index.Count);

            IImmutableValueSet<Finisher> finishers = await _gw2Client.Hero.Equipment.Finishers
                .GetFinishersByIds(index, language, MissingMemberBehavior.Undefined, cancellationToken)
                .ValueOnly().ConfigureAwait(false);

            await context.AddRangeAsync(finishers, cancellationToken).ConfigureAwait(false);
            _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} finishers.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedGliders(ChatLinksContext context, Language language, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding gliders.");

        IImmutableValueSet<int> index = await _gw2Client.Hero.Equipment.Gliders
            .GetGliderSkinsIndex(cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        _logger.LogDebug("Found {Count} gliders in the API.", index.Count);
        List<int> existing = await context.Gliders.Select(glider => glider.Id)
            .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        index = index.Except(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} gliders.", index.Count);

            IImmutableValueSet<GliderSkin> gliders = await _gw2Client.Hero.Equipment.Gliders
                .GetGliderSkinsByIds(index, language, MissingMemberBehavior.Undefined, cancellationToken)
                .ValueOnly().ConfigureAwait(false);

            await context.AddRangeAsync(gliders, cancellationToken).ConfigureAwait(false);
            _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} gliders.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedJadeBots(ChatLinksContext context, Language language, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding jade bots.");

        IImmutableValueSet<int> index = await _gw2Client.Hero.Equipment.JadeBots
            .GetJadeBotSkinsIndex(cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        _logger.LogDebug("Found {Count} jade bots in the API.", index.Count);
        List<int> existing = await context.JadeBots.Select(jadeBot => jadeBot.Id)
            .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        index = index.Except(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} jade bots.", index.Count);

            IImmutableValueSet<JadeBotSkin> jadeBots = await _gw2Client.Hero.Equipment.JadeBots
                .GetJadeBotSkinsByIds(index, language, MissingMemberBehavior.Undefined, cancellationToken)
                .ValueOnly().ConfigureAwait(false);

            await context.AddRangeAsync(jadeBots, cancellationToken).ConfigureAwait(false);
            _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} jade bots.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedMailCarriers(ChatLinksContext context, Language language,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding mail carriers.");

        IImmutableValueSet<int> index = await _gw2Client.Hero.Equipment.MailCarriers
            .GetMailCarriersIndex(cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        _logger.LogDebug("Found {Count} mail carriers in the API.", index.Count);
        List<int> existing = await context.MailCarrriers.Select(mailCarrier => mailCarrier.Id)
            .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        index = index.Except(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} mail carriers.", index.Count);

            IImmutableValueSet<MailCarrier> mailCarriers = await _gw2Client.Hero.Equipment.MailCarriers
                .GetMailCarriersByIds(index, language, MissingMemberBehavior.Undefined, cancellationToken)
                .ValueOnly().ConfigureAwait(false);

            await context.AddRangeAsync(mailCarriers, cancellationToken).ConfigureAwait(false);
            _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} mail carriers.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedMiniatures(ChatLinksContext context, Language language,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding miniatures.");

        IImmutableValueSet<int> index = await _gw2Client.Hero.Equipment.Miniatures
            .GetMiniaturesIndex(cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        _logger.LogDebug("Found {Count} miniatures in the API.", index.Count);
        List<int> existing = await context.Miniatures.Select(miniature => miniature.Id)
            .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        index = index.Except(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} miniatures.", index.Count);

            // TODO: incremental query
            IImmutableValueSet<GuildWars2.Hero.Equipment.Miniatures.Miniature> miniatures = await _gw2Client.Hero.Equipment.Miniatures
                .GetMiniatures(language, MissingMemberBehavior.Undefined, cancellationToken).ValueOnly().ConfigureAwait(false);

            await context.AddRangeAsync(miniatures.Where(miniature => index.Contains(miniature.Id)), cancellationToken).ConfigureAwait(false);
            _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} miniatures.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedMistChampions(ChatLinksContext context, Language language,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding mist champions.");

        IImmutableValueSet<MistChampion> champions = await _gw2Client.Pvp
            .GetMistChampions(language, MissingMemberBehavior.Undefined, cancellationToken: cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        IImmutableValueSet<int> index = [.. champions.SelectMany(champion => champion.Skins.Select(skin => skin.Id))];

        _logger.LogDebug("Found {Count} mist champions in the API.", index.Count);
        List<int> existing = await context.MistChampions.Select(mistChampion => mistChampion.Id)
            .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        index = index.Except(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} mist champions.", index.Count);
            List<MistChampionSkin> mistChampions = [.. champions
                .SelectMany(champion => champion.Skins)
                .Where(skin => index.Contains(skin.Id))];

            await context.AddRangeAsync(mistChampions, cancellationToken).ConfigureAwait(false);
            _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} mist champions.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedNovelties(ChatLinksContext context, Language language,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding novelties.");

        IImmutableValueSet<int> index = await _gw2Client.Hero.Equipment.Novelties
            .GetNoveltiesIndex(cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        _logger.LogDebug("Found {Count} novelties in the API.", index.Count);
        List<int> existing = await context.Novelties.Select(novelty => novelty.Id)
            .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        index = index.Except(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} novelties.", index.Count);

            // TODO: incremental query
            IImmutableValueSet<Novelty> novelties = await _gw2Client.Hero.Equipment.Novelties
                .GetNovelties(language, MissingMemberBehavior.Undefined, cancellationToken).ValueOnly().ConfigureAwait(false);

            await context.AddRangeAsync(novelties.Where(novelty => index.Contains(novelty.Id)), cancellationToken).ConfigureAwait(false);
            _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} novelties.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedOutfits(ChatLinksContext context, Language language,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding outfits.");

        IImmutableValueSet<int> index = await _gw2Client.Hero.Equipment.Outfits
            .GetOutfitsIndex(cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        _logger.LogDebug("Found {Count} outfits in the API.", index.Count);
        List<int> existing = await context.Outfits.Select(outfit => outfit.Id)
            .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        index = index.Except(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} outfits.", index.Count);

            // TODO: incremental query
            IImmutableValueSet<Outfit> outfits = await _gw2Client.Hero.Equipment.Outfits
                .GetOutfits(language, MissingMemberBehavior.Undefined, cancellationToken).ValueOnly().ConfigureAwait(false);

            await context.AddRangeAsync(outfits.Where(outfit => index.Contains(outfit.Id)), cancellationToken).ConfigureAwait(false);
            _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} outfits.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedAchievements(ChatLinksContext context, Language language,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding achievements.");

        IImmutableValueSet<int> index = await _gw2Client.Hero.Achievements
            .GetAchievementsIndex(cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        _logger.LogDebug("Found {Count} achievements in the API.", index.Count);
        List<int> existing = await context.Achievements.Select(achievement => achievement.Id)
            .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        index = index.Except(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} achievements.", index.Count);
            Progress<BulkProgress> bulkProgress = new(report =>
            {
                _eventAggregator.Publish(new DatabaseSyncProgress("achievements", report));
            });

            int count = 0;
            await foreach (Achievement achievement in _gw2Client.Hero.Achievements
                .GetAchievementsBulk(
                    index,
                    language,
                    MissingMemberBehavior.Undefined,
                    3,
                    200,
                    bulkProgress,
                    cancellationToken
                )
                .ValueOnly(cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                _ = context.Add(achievement);
                if (++count % 333 == 0)
                {
                    _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    DetachAllEntities(context);
                }
            }

            _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} achievements.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedAchievementCategories(ChatLinksContext context, Language language,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding achievement categories.");

        IImmutableValueSet<int> index = await _gw2Client.Hero.Achievements
            .GetAchievementCategoriesIndex(cancellationToken)
            .ValueOnly().ConfigureAwait(false);

        _logger.LogDebug("Found {Count} achievement categories in the API.", index.Count);
        List<int> existing = await context.AchievementCategories.Select(category => category.Id)
            .ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        _logger.LogDebug("Start seeding {Count} achievement categories.", index.Count);
        IImmutableValueSet<AchievementCategory> achievementCategories = await _gw2Client.Hero.Achievements
            .GetAchievementCategories(language, MissingMemberBehavior.Undefined, cancellationToken)
            .ValueOnly()
            .ConfigureAwait(false);

        foreach (AchievementCategory category in achievementCategories)
        {
            context.Entry(category).State = existing.Contains(category.Id)
                ? EntityState.Modified
                : EntityState.Added;
        }

        _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        DetachAllEntities(context);

        _logger.LogInformation("Finished seeding {Count} achievement categories.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedAchievementGroups(ChatLinksContext context, Language language,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding achievement groups.");

        IImmutableValueSet<string> index = await _gw2Client.Hero.Achievements
            .GetAchievementGroupsIndex(cancellationToken)
            .ValueOnly()
            .ConfigureAwait(false);

        _logger.LogDebug("Found {Count} achievement groups in the API.", index.Count);
        List<string> existing = await context.AchievementGroups.Select(group => group.Id)
            .ToListAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        _logger.LogDebug("Start seeding {Count} achievement groups.", index.Count);
        IImmutableValueSet<AchievementGroup> achievementGroups = await _gw2Client.Hero.Achievements
            .GetAchievementGroups(language, MissingMemberBehavior.Undefined, cancellationToken)
            .ValueOnly()
            .ConfigureAwait(false);

        foreach (AchievementGroup group in achievementGroups)
        {
            context.Entry(group).State = existing.Contains(group.Id)
                ? EntityState.Modified
                : EntityState.Added;
        }

        _ = await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        DetachAllEntities(context);

        _logger.LogInformation("Finished seeding {Count} achievement groups.", index.Count);
        return index.Count;
    }

    private static void DetachAllEntities(ChatLinksContext context)
    {
        List<EntityEntry> entries = [.. context.ChangeTracker.Entries()];
        foreach (EntityEntry? entry in entries)
        {
            entry.State = EntityState.Detached;
        }
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<HourStarted>(OnHourStarted);
        _eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
        _syncSemaphore.Dispose();
    }
}
