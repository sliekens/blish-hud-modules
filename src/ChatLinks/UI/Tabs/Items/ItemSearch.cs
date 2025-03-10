﻿using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using GuildWars2.Chat;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;

using SL.ChatLinks.Storage;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed record ResultContext
{
    public int ResultTotal { get; set; }
}

public sealed class ItemSearch(IDbContextFactory contextFactory, ILocale locale)
{
    private static readonly Regex ChatLinkPattern = new(@"^\[&[A-Za-z0-9+/=]+\]$", RegexOptions.Compiled);

    public async ValueTask<int> CountItems()
    {
        ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
        await using (context.ConfigureAwait(false))
        {
            return await context.Items.CountAsync().ConfigureAwait(false);
        }
    }

    public async IAsyncEnumerable<Item> NewItems(int limit)
    {
        ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
        await using (context.ConfigureAwait(false))
        {
            await foreach (Item? item in context.Items
               .OrderByDescending(item => item.Id)
               .Take(limit)
               .AsAsyncEnumerable().ConfigureAwait(false))
            {
                yield return item;
            }
        }
    }

    public async IAsyncEnumerable<Item> Search(
        string searchText,
        int limit,
        ResultContext resultContext,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ThrowHelper.ThrowIfNull(resultContext);
        if (ChatLinkPattern.IsMatch(searchText))
        {
            ItemLink chatLink = ItemLink.Parse(searchText);
            await foreach (Item item in SearchByChatLink(chatLink, resultContext, cancellationToken).ConfigureAwait(false))
            {
                yield return item;
            }
        }
        else
        {
            ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
            await using (context.ConfigureAwait(false))
            {
                IQueryable<Item> query = context.Items.FromSqlInterpolated(
                    $"""
                 SELECT * FROM Items
                 WHERE Name LIKE '%' || {searchText} || '%'
                 ORDER BY LevenshteinDistance({searchText}, Name)
                 """);

                resultContext.ResultTotal = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

                await foreach (Item? item in query
                   .Take(limit)
                   .AsAsyncEnumerable()
                   .WithCancellation(cancellationToken))
                {
                    yield return item;
                }
            }
        }
    }

    private async IAsyncEnumerable<Item> SearchByChatLink(
        ItemLink link,
        ResultContext resultContext,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
        await using (context.ConfigureAwait(false))
        {
            Item item = await context.Items
                .SingleOrDefaultAsync(row => row.Id == link.ItemId, cancellationToken).ConfigureAwait(false);
            if (item is null)
            {
                yield break;
            }

            resultContext.ResultTotal++;
            yield return item;

            HashSet<int> relatedItems = [];
            if (link.SuffixItemId.HasValue)
            {
                _ = relatedItems.Add(link.SuffixItemId.Value);
            }

            if (link.SecondarySuffixItemId.HasValue)
            {
                _ = relatedItems.Add(link.SecondarySuffixItemId.Value);
            }

            switch (item)
            {
                case Weapon weapon:
                    if (weapon.SuffixItemId.HasValue)
                    {
                        _ = relatedItems.Add(weapon.SuffixItemId.Value);
                    }

                    if (weapon.SecondarySuffixItemId.HasValue)
                    {
                        _ = relatedItems.Add(weapon.SecondarySuffixItemId.Value);
                    }

                    foreach (InfusionSlot? slot in weapon.InfusionSlots)
                    {
                        if (slot.ItemId.HasValue)
                        {
                            _ = relatedItems.Add(slot.ItemId.Value);
                        }
                    }

                    break;

                case Armor armor:
                    if (armor.SuffixItemId.HasValue)
                    {
                        _ = relatedItems.Add(armor.SuffixItemId.Value);
                    }

                    foreach (InfusionSlot? slot in armor.InfusionSlots)
                    {
                        if (slot.ItemId.HasValue)
                        {
                            _ = relatedItems.Add(slot.ItemId.Value);
                        }
                    }

                    break;
                case Backpack back:
                    if (back.SuffixItemId.HasValue)
                    {
                        _ = relatedItems.Add(back.SuffixItemId.Value);
                    }

                    foreach (InfusionSlot? slot in back.InfusionSlots)
                    {
                        if (slot.ItemId.HasValue)
                        {
                            _ = relatedItems.Add(slot.ItemId.Value);
                        }
                    }

                    foreach (InfusionSlotUpgradeSource? source in back.UpgradesFrom)
                    {
                        _ = relatedItems.Add(source.ItemId);
                    }

                    foreach (InfusionSlotUpgradePath? upgrade in back.UpgradesInto)
                    {
                        _ = relatedItems.Add(upgrade.ItemId);
                    }

                    break;

                case Trinket trinket:
                    if (trinket.SuffixItemId.HasValue)
                    {
                        _ = relatedItems.Add(trinket.SuffixItemId.Value);
                    }

                    foreach (InfusionSlot? slot in trinket.InfusionSlots)
                    {
                        if (slot.ItemId.HasValue)
                        {
                            _ = relatedItems.Add(slot.ItemId.Value);
                        }
                    }

                    break;
                case CraftingMaterial material:

                    foreach (InfusionSlotUpgradePath? upgrade in material.UpgradesInto)
                    {
                        _ = relatedItems.Add(upgrade.ItemId);
                    }

                    break;
                default:
                    break;
            }

            resultContext.ResultTotal += relatedItems.Count;
            await foreach (Item? relatedItem in context.Items
                               .Where(i => relatedItems.Contains(i.Id))
                               .AsAsyncEnumerable()
                               .WithCancellation(cancellationToken))
            {
                yield return relatedItem;
            }
        }
    }
}
