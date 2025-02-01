using System.Globalization;

using Blish_HUD.Modules;

using System.IO.Compression;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using SL.ChatLinks.Storage;
using SL.Common;

using GuildWars2.Items;
using GuildWars2;
using GuildWars2.Hero.Equipment.Wardrobe;

using Microsoft.Extensions.Logging;

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
        await Migrate(args.CultureInfo);
        await Sync(args.CultureInfo, CancellationToken.None);
    }

    public async Task Migrate(CultureInfo culture)
    {
        var fileName = _options.Value.DatabaseFileName(culture);
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

        await using var context = _contextFactory.CreateDbContext(culture);
        await context.Database.MigrateAsync();
    }

    public async Task Sync(CultureInfo culture, CancellationToken cancellationToken)
    {
        await using var context = _contextFactory.CreateDbContext(culture);
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        await Seed(context, culture, cancellationToken);
    }

    private async Task Seed(ChatLinksContext context, CultureInfo culture, CancellationToken cancellationToken)
    {
        Language language = new(culture.TwoLetterISOLanguageName);
        await SeedItems(context, language, cancellationToken);
        await SeedSkins(context, language, cancellationToken);
        await SeedColors(context, language, cancellationToken);
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

            context.ChangeTracker.AutoDetectChangesEnabled = false;
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

            context.ChangeTracker.AutoDetectChangesEnabled = false;
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

            foreach(var color in colors.Where(color => index.Contains(color.Id)))
            {
                context.Add(color);
            }

            await context.AddRangeAsync(colors, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            DetachAllEntities(context); 
        }

        _logger.LogInformation("Finished seeding {Count} colors.", index.Count);
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