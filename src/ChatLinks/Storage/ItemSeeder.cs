using System;

using GuildWars2;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SL.ChatLinks.Storage;

public sealed class ItemSeeder(
    ILogger<ItemSeeder> logger,
    ChatLinksContext context,
    Gw2Client gw2Client)
{
    public async Task Seed(IProgress<BulkProgress> progress, CancellationToken cancellationToken)
    {
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
            const int batchSize = 100;
            List<Item> batch = new(batchSize);
            await foreach (Item item in gw2Client.Items
                               .GetItemsBulk(index, progress: progress, cancellationToken: cancellationToken)
                               .ValueOnly(cancellationToken: cancellationToken))
            {
                if (batch.Count == batchSize)
                {
                    await context.BulkInsertAsync(batch, o =>
                    {
                        o.BatchSize = batchSize;
                        o.InsertKeepIdentity = true;
                    }, cancellationToken);
                    batch.Clear();
                }

                batch.Add(item);
            }

            if (batch.Count != 0)
            {
                await context.BulkInsertAsync(batch, o =>
                {
                    o.BatchSize = batchSize;
                    o.InsertKeepIdentity = true;
                }, cancellationToken);
            }
        }

        logger.LogInformation("Finished seeding {Count} items.", index.Count);
    }

}