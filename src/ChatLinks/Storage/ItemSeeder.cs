

using System.Globalization;

using GuildWars2;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SL.Common;

namespace SL.ChatLinks.Storage;

public sealed class ItemSeeder(
    ILogger<ItemSeeder> logger,
    IDbContextFactory contextFactory,
    Gw2Client gw2Client,
    IEventAggregator eventAggregator
)
{
    public async Task Seed(CancellationToken cancellationToken)
    {
        await using var context = contextFactory.CreateDbContext(CultureInfo.CurrentUICulture);
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        logger.LogInformation("Start seeding items.");

        HashSet<int> index = await gw2Client.Items
            .GetItemsIndex(cancellationToken)
            .ValueOnly();

        logger.LogDebug("Found {Count} items in the API.", index.Count);
        var existing = await context.Items.Select(item => item.Id)
            .ToListAsync(cancellationToken: cancellationToken);

        index.ExceptWith(existing);
        if (index.Count != 0)
        {
            logger.LogDebug("Start seeding {Count} items.", index.Count);
            Progress<BulkProgress> bulkProgress = new(report =>
            {
                eventAggregator.Publish(new DatabaseSyncProgress(report));
            });

            context.ChangeTracker.AutoDetectChangesEnabled = false;
            var count = 0;
            await foreach (Item item in gw2Client.Items
                               .GetItemsBulk(index,
                                   null,
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

        logger.LogInformation("Finished seeding {Count} items.", index.Count);
        ThreadPool.QueueUserWorkItem(_ =>
        {
            eventAggregator.Publish(new DatabaseSyncCompleted());
        });
    }

    private static void DetachAllEntities(ChatLinksContext context)
    {
        var entries = context.ChangeTracker.Entries().ToList();
        foreach (var entry in entries)
        {
            entry.State = EntityState.Detached;
        }
    }

}