using System.Globalization;

using Blish_HUD.Modules;

using System.IO.Compression;

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

namespace SL.ChatLinks;

public sealed class DatabaseSeeder : IDisposable
{
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly IOptions<DatabaseOptions> _options;
    private readonly IDbContextFactory _contextFactory;
    private readonly IEventAggregator _eventAggregator;
    private readonly ModuleParameters _moduleParameters;
    private readonly Gw2Client _gw2Client;

    public DatabaseSeeder(ILogger<DatabaseSeeder> logger,
        IOptions<DatabaseOptions> options,
        IDbContextFactory contextFactory,
        IEventAggregator eventAggregator,
        ModuleParameters moduleParameters,
        Gw2Client gw2Client)
    {
        _logger = logger;
        _options = options;
        _contextFactory = contextFactory;
        _eventAggregator = eventAggregator;
        _moduleParameters = moduleParameters;
        _gw2Client = gw2Client;

        eventAggregator.Subscribe<LocaleChanged>(OnLocaleChanged);
    }

    private async ValueTask OnLocaleChanged(LocaleChanged args)
    {
        await Migrate(args.Language);
        await Sync(args.Language, CancellationToken.None);
    }

    public async Task Migrate(Language language)
    {
        var fileName = _options.Value.DatabaseFileName(language);
        var location = Path.Combine(_options.Value.Directory, fileName);
        if (new FileInfo(location) is { Exists: false } or { Length: 0 })
        {
            using var seed = _moduleParameters.ContentsManager.GetFileStream(_options.Value.RefData);
            using var unzip = new ZipArchive(seed, ZipArchiveMode.Read);
            var data = unzip.GetEntry(fileName);
            if (data is not null)
            {
                using var dataStream = data.Open();
                using var fileStream = File.Create(location);
                await dataStream.CopyToAsync(fileStream);
            }
        }

        await using var context = _contextFactory.CreateDbContext(language);
        await context.Database.MigrateAsync();
    }

    public async Task Sync(Language language, CancellationToken cancellationToken)
    {
        await using var context = _contextFactory.CreateDbContext(language);
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        await Seed(context, language, cancellationToken);
    }

    private async Task Seed(ChatLinksContext context, Language language, CancellationToken cancellationToken)
    {
        await SeedItems(context, language, cancellationToken);
        await SeedSkins(context, language, cancellationToken);
        await SeedRecipes(context, language, cancellationToken);
        await SeedColors(context, language, cancellationToken);
        await SeedFinishers(context, language, cancellationToken);
        await SeedGliders(context, language, cancellationToken);
        await SeedJadeBots(context, language, cancellationToken);
        await SeedMailCarriers(context, language, cancellationToken);
        await SeedMistChampions(context, language, cancellationToken);
        await SeedNovelties(context, language, cancellationToken);
        await SeedOutfits(context, language, cancellationToken);
        await _eventAggregator.PublishAsync(new DatabaseSyncCompleted(), cancellationToken);
    }

    private async Task SeedItems(ChatLinksContext context, Language language, CancellationToken cancellationToken)
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
    }

    private async Task SeedSkins(ChatLinksContext context, Language language, CancellationToken cancellationToken)
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
    }

    private async Task SeedColors(ChatLinksContext context, Language language, CancellationToken cancellationToken)
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
    }

    private async Task SeedRecipes(ChatLinksContext context, Language language, CancellationToken cancellationToken)
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
    }

    private async Task SeedFinishers(ChatLinksContext context, Language language, CancellationToken cancellationToken)
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
    }

    private async Task SeedGliders(ChatLinksContext context, Language language, CancellationToken cancellationToken)
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
    }

    private async Task SeedJadeBots(ChatLinksContext context, Language language, CancellationToken cancellationToken)
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
    }

    private async Task SeedMailCarriers(ChatLinksContext context, Language language,
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
    }

    private async Task SeedMistChampions(ChatLinksContext context, Language language,
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
    }

    private async Task SeedNovelties(ChatLinksContext context, Language language,
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
    }

    private async Task SeedOutfits(ChatLinksContext context, Language language,
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