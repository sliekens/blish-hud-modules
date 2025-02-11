using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using SL.ChatLinks.Storage;
using SL.Common;

using GuildWars2.Items;
using GuildWars2;
using GuildWars2.Hero.Crafting.Recipes;
using GuildWars2.Hero.Equipment.Wardrobe;

using Microsoft.Extensions.Logging;

using GuildWars2.Pvp.MistChampions;
using SL.ChatLinks.Storage.Metadata;

using Language = GuildWars2.Language;
using SL.ChatLinks.StaticFiles;

namespace SL.ChatLinks;

public sealed class DatabaseSeeder : IDisposable
{
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly IOptions<DatabaseOptions> _options;
    private readonly IDbContextFactory _contextFactory;
    private readonly IEventAggregator _eventAggregator;
    private readonly Gw2Client _gw2Client;
    private readonly StaticDataClient _staticDataClient;

    public DatabaseSeeder(ILogger<DatabaseSeeder> logger,
        IOptions<DatabaseOptions> options,
        IDbContextFactory contextFactory,
        IEventAggregator eventAggregator,
        Gw2Client gw2Client,
        StaticDataClient staticDataClient
    )
    {
        _logger = logger;
        _options = options;
        _contextFactory = contextFactory;
        _eventAggregator = eventAggregator;
        _gw2Client = gw2Client;
        _staticDataClient = staticDataClient;
        eventAggregator.Subscribe<LocaleChanged>(OnLocaleChanged);
    }

    private async ValueTask OnLocaleChanged(LocaleChanged args)
    {
        await Task.Run(async () =>
        {
            await Migrate(args.Language);
            await Sync(args.Language, CancellationToken.None);
        });
    }

    private async ValueTask<DataManifest?> DataManifest()
    {
        try
        {
            var path = Path.Combine(_options.Value.Directory, "manifest.json");
            if (!File.Exists(path))
            {
                return null;
            }

            using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<DataManifest>(stream);
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
            var path = Path.Combine(_options.Value.Directory, "manifest.json");
            using var stream = File.OpenWrite(path);
            await JsonSerializer.SerializeAsync(stream, manifest);
        }
        catch (Exception reason)
        {
            _logger.LogError(reason, "Failed to save updated manifest.");
        }
    }

    private bool IsEmpty(Database database)
    {
        var location = Path.Combine(_options.Value.Directory, database.Name);
        return new FileInfo(location) is { Exists: false } or { Length: 0 };
    }

    public async Task Migrate(Language language)
    {
        DataManifest currentDataManifest = await DataManifest() ?? new DataManifest
        {
            Version = 1,
            Databases = []
        };

        bool shouldDownload = !currentDataManifest.Databases.TryGetValue(language.Alpha2Code, out Database? currentDatabase)
            || currentDatabase.SchemaVersion > ChatLinksContext.SchemaVersion
            || IsEmpty(currentDatabase);

        if (shouldDownload)
        {
            var seedDatabase = await DownloadDatabase(language);
            if (seedDatabase is not null)
            {
                currentDatabase = seedDatabase;
                currentDataManifest.Databases[language.Alpha2Code] = seedDatabase;

                await SaveManifest(currentDataManifest);

                await _eventAggregator.PublishAsync(new DatabaseDownloaded());
            }
        }

        if (currentDatabase is null)
        {
            _logger.LogWarning("No usable database found for language {Language}.", language.Alpha2Code);
            return;
        }

        await using var context = _contextFactory.CreateDbContext(currentDatabase.Name);
        await context.Database.MigrateAsync();
    }

    private async Task<Database?> DownloadDatabase(Language language)
    {
        var seedDataManifest = await _staticDataClient.GetSeedIndex(CancellationToken.None);
        var seedDatabase = seedDataManifest.Databases
            .SingleOrDefault(seed => seed.SchemaVersion == ChatLinksContext.SchemaVersion && seed.Language == language.Alpha2Code);

        if (seedDatabase is null)
        {
            return null;
        }

        var destination = Path.Combine(_options.Value.Directory, seedDatabase.Name);
        await _staticDataClient.Download(seedDatabase, destination, CancellationToken.None);

        return new Database
        {
            Name = seedDatabase.Name,
            SchemaVersion = seedDatabase.SchemaVersion
        };
    }

    public async Task Sync(Language language, CancellationToken cancellationToken)
    {
        await using var context = _contextFactory.CreateDbContext(language);
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        await Seed(context, language, cancellationToken);
    }

    public async Task SeedAll()
    {
        Directory.CreateDirectory(_options.Value.Directory);
        var manifest = new DataManifest
        {
            Version = 1,
            Databases = []
        };

        foreach (var language in (Language[])[Language.English, Language.German, Language.French, Language.Spanish])
        {
            Database database = new()
            {
                Name = _options.Value.DatabaseFileName(language),
                SchemaVersion = ChatLinksContext.SchemaVersion
            };

            await using var context = _contextFactory.CreateDbContext(database.Name);
            await context.Database.MigrateAsync();
            await Seed(context, language, CancellationToken.None);

            manifest.Databases[language.Alpha2Code] = database;
        }

        await SaveManifest(manifest);
    }

    public async Task Seed(ChatLinksContext context, Language language, CancellationToken cancellationToken)
    {
        var inserted = new Dictionary<string, int>
        {
            ["items"] = await SeedItems(context, language, cancellationToken),
            ["skins"] = await SeedSkins(context, language, cancellationToken),
            ["recipes"] = await SeedRecipes(context, language, cancellationToken),
            ["colors"] = await SeedColors(context, language, cancellationToken),
            ["finishers"] = await SeedFinishers(context, language, cancellationToken),
            ["gliders"] = await SeedGliders(context, language, cancellationToken),
            ["jadeBots"] = await SeedJadeBots(context, language, cancellationToken),
            ["mailCarriers"] = await SeedMailCarriers(context, language, cancellationToken),
            ["miniatures"] = await SeedMiniatures(context, language, cancellationToken),
            ["mistChampions"] = await SeedMistChampions(context, language, cancellationToken),
            ["novelties"] = await SeedNovelties(context, language, cancellationToken),
            ["outfits"] = await SeedOutfits(context, language, cancellationToken)
        };

        await _eventAggregator.PublishAsync(new DatabaseSyncCompleted(inserted), cancellationToken);
    }

    private async Task<int> SeedItems(ChatLinksContext context, Language language, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding items.");

        HashSet<int> index = await _gw2Client.Items
            .GetItemsIndex(cancellationToken)
            .ValueOnly();

        _logger.LogDebug("Found {Count} items in the API.", index.Count);
        var existing = await context.Items.Select(item => item.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        index.ExceptWith(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} items.", index.Count);
            Progress<BulkProgress> bulkProgress = new(report =>
            {
                _eventAggregator.Publish(new DatabaseSyncProgress("items", report));
            });

            var count = 0;
            await foreach (Item item in _gw2Client.Items
                               .GetItemsBulk(index,
                                   language,
                                   MissingMemberBehavior.Undefined,
                                   3,
                                   200,
                                   bulkProgress,
                                   cancellationToken)
                               .ValueOnly(cancellationToken: cancellationToken))
            {
                context.Add(item);
                if (++count % 333 == 0)
                {
                    await context.SaveChangesAsync(cancellationToken);
                    DetachAllEntities(context);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} items.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedSkins(ChatLinksContext context, Language language, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding skins.");

        HashSet<int> index = await _gw2Client.Hero.Equipment.Wardrobe
            .GetSkinsIndex(cancellationToken)
            .ValueOnly();

        _logger.LogDebug("Found {Count} skins in the API.", index.Count);
        var existing = await context.Skins.Select(skin => skin.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        index.ExceptWith(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} skins.", index.Count);
            Progress<BulkProgress> bulkProgress = new(report =>
            {
                _eventAggregator.Publish(new DatabaseSyncProgress("skins", report));
            });

            var count = 0;
            await foreach (EquipmentSkin skin in _gw2Client.Hero.Equipment.Wardrobe
                               .GetSkinsBulk(index,
                                   language,
                                   MissingMemberBehavior.Undefined,
                                   3,
                                   200,
                                   bulkProgress,
                                   cancellationToken)
                               .ValueOnly(cancellationToken: cancellationToken))
            {
                context.Add(skin);
                if (++count % 333 == 0)
                {
                    await context.SaveChangesAsync(cancellationToken);
                    DetachAllEntities(context);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} skins.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedColors(ChatLinksContext context, Language language, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding colors.");

        HashSet<int> index = await _gw2Client.Hero.Equipment.Dyes
            .GetColorsIndex(cancellationToken)
            .ValueOnly();

        _logger.LogDebug("Found {Count} colors in the API.", index.Count);
        var existing = await context.Colors.Select(color => color.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        index.ExceptWith(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} colors.", index.Count);

            // TODO: incremental query
            var colors = await _gw2Client.Hero.Equipment.Dyes
                .GetColors(language, MissingMemberBehavior.Undefined, cancellationToken).ValueOnly();

            await context.AddRangeAsync(colors.Where(color => index.Contains(color.Id)), cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} colors.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedRecipes(ChatLinksContext context, Language language, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding recipes.");

        HashSet<int> index = await _gw2Client.Hero.Crafting.Recipes
            .GetRecipesIndex(cancellationToken)
            .ValueOnly();

        _logger.LogDebug("Found {Count} recipes in the API.", index.Count);
        var existing = await context.Recipes.Select(recipe => recipe.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        index.ExceptWith(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} recipes.", index.Count);
            Progress<BulkProgress> bulkProgress = new(report =>
            {
                _eventAggregator.Publish(new DatabaseSyncProgress("recipes", report));
            });

            var count = 0;
            await foreach (Recipe recipe in _gw2Client.Hero.Crafting.Recipes
                               .GetRecipesBulk(index,
                                   MissingMemberBehavior.Undefined,
                                   3,
                                   200,
                                   bulkProgress,
                                   cancellationToken)
                               .ValueOnly(cancellationToken: cancellationToken))
            {
                context.Add(recipe);
                if (++count % 333 == 0)
                {
                    await context.SaveChangesAsync(cancellationToken);
                    DetachAllEntities(context);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} recipes.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedFinishers(ChatLinksContext context, Language language, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding finishers.");

        HashSet<int> index = await _gw2Client.Hero.Equipment.Finishers
            .GetFinishersIndex(cancellationToken)
            .ValueOnly();

        _logger.LogDebug("Found {Count} finishers in the API.", index.Count);
        var existing = await context.Finishers.Select(finisher => finisher.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        index.ExceptWith(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} finishers.", index.Count);

            var finishers = await _gw2Client.Hero.Equipment.Finishers
                .GetFinishersByIds(index, language, MissingMemberBehavior.Undefined, cancellationToken)
                .ValueOnly();

            await context.AddRangeAsync(finishers, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} finishers.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedGliders(ChatLinksContext context, Language language, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding gliders.");

        HashSet<int> index = await _gw2Client.Hero.Equipment.Gliders
            .GetGliderSkinsIndex(cancellationToken)
            .ValueOnly();

        _logger.LogDebug("Found {Count} gliders in the API.", index.Count);
        var existing = await context.Gliders.Select(glider => glider.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        index.ExceptWith(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} gliders.", index.Count);

            var gliders = await _gw2Client.Hero.Equipment.Gliders
                .GetGliderSkinsByIds(index, language, MissingMemberBehavior.Undefined, cancellationToken)
                .ValueOnly();

            await context.AddRangeAsync(gliders, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} gliders.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedJadeBots(ChatLinksContext context, Language language, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding jade bots.");

        HashSet<int> index = await _gw2Client.Hero.Equipment.JadeBots
            .GetJadeBotSkinsIndex(cancellationToken)
            .ValueOnly();

        _logger.LogDebug("Found {Count} jade bots in the API.", index.Count);
        var existing = await context.JadeBots.Select(jadeBot => jadeBot.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        index.ExceptWith(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} jade bots.", index.Count);

            var jadeBots = await _gw2Client.Hero.Equipment.JadeBots
                .GetJadeBotSkinsByIds(index, language, MissingMemberBehavior.Undefined, cancellationToken)
                .ValueOnly();

            await context.AddRangeAsync(jadeBots, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} jade bots.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedMailCarriers(ChatLinksContext context, Language language,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding mail carriers.");

        HashSet<int> index = await _gw2Client.Hero.Equipment.MailCarriers
            .GetMailCarriersIndex(cancellationToken)
            .ValueOnly();

        _logger.LogDebug("Found {Count} mail carriers in the API.", index.Count);
        var existing = await context.MailCarrriers.Select(mailCarrier => mailCarrier.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        index.ExceptWith(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} mail carriers.", index.Count);

            var mailCarriers = await _gw2Client.Hero.Equipment.MailCarriers
                .GetMailCarriersByIds(index, language, MissingMemberBehavior.Undefined, cancellationToken)
                .ValueOnly();

            await context.AddRangeAsync(mailCarriers, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} mail carriers.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedMiniatures(ChatLinksContext context, Language language,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding miniatures.");

        HashSet<int> index = await _gw2Client.Hero.Equipment.Miniatures
            .GetMiniaturesIndex(cancellationToken)
            .ValueOnly();

        _logger.LogDebug("Found {Count} miniatures in the API.", index.Count);
        var existing = await context.Miniatures.Select(miniature => miniature.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        index.ExceptWith(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} miniatures.", index.Count);

            // TODO: incremental query
            var miniatures = await _gw2Client.Hero.Equipment.Miniatures
                .GetMiniatures(language, MissingMemberBehavior.Undefined, cancellationToken).ValueOnly();

            await context.AddRangeAsync(miniatures.Where(miniature => index.Contains(miniature.Id)), cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} miniatures.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedMistChampions(ChatLinksContext context, Language language,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding mist champions.");

        HashSet<MistChampion> champions = await _gw2Client.Pvp
            .GetMistChampions(language, MissingMemberBehavior.Undefined, cancellationToken: cancellationToken)
            .ValueOnly();

        var index = champions.SelectMany(champion => champion.Skins.Select(skin => skin.Id)).ToHashSet();

        _logger.LogDebug("Found {Count} mist champions in the API.", index.Count);
        var existing = await context.MistChampions.Select(mistChampion => mistChampion.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        index.ExceptWith(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} mist champions.", index.Count);
            var mistChampions = champions
                .SelectMany(champion => champion.Skins)
                .Where(skin => index.Contains(skin.Id))
                .ToList();

            await context.AddRangeAsync(mistChampions, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} mist champions.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedNovelties(ChatLinksContext context, Language language,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding novelties.");

        HashSet<int> index = await _gw2Client.Hero.Equipment.Novelties
            .GetNoveltiesIndex(cancellationToken)
            .ValueOnly();

        _logger.LogDebug("Found {Count} novelties in the API.", index.Count);
        var existing = await context.Novelties.Select(novelty => novelty.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        index.ExceptWith(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} novelties.", index.Count);

            // TODO: incremental query
            var novelties = await _gw2Client.Hero.Equipment.Novelties
                .GetNovelties(language, MissingMemberBehavior.Undefined, cancellationToken).ValueOnly();

            await context.AddRangeAsync(novelties.Where(novelty => index.Contains(novelty.Id)), cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} novelties.", index.Count);
        return index.Count;
    }

    private async Task<int> SeedOutfits(ChatLinksContext context, Language language,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start seeding outfits.");

        HashSet<int> index = await _gw2Client.Hero.Equipment.Outfits
            .GetOutfitsIndex(cancellationToken)
            .ValueOnly();

        _logger.LogDebug("Found {Count} outfits in the API.", index.Count);
        var existing = await context.Outfits.Select(outfit => outfit.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        index.ExceptWith(existing);
        if (index.Count != 0)
        {
            _logger.LogDebug("Start seeding {Count} outfits.", index.Count);

            // TODO: incremental query
            var outfits = await _gw2Client.Hero.Equipment.Outfits
                .GetOutfits(language, MissingMemberBehavior.Undefined, cancellationToken).ValueOnly();

            await context.AddRangeAsync(outfits.Where(outfit => index.Contains(outfit.Id)), cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            DetachAllEntities(context);
        }

        _logger.LogInformation("Finished seeding {Count} outfits.", index.Count);
        return index.Count;
    }

    private static void DetachAllEntities(ChatLinksContext context)
    {
        var entries = context.ChangeTracker.Entries().ToList();
        foreach (var entry in entries)
        {
            entry.State = EntityState.Detached;
        }
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
    }
}