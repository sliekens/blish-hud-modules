﻿using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using GuildWars2.Chat;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;

using SL.ChatLinks.Storage;

namespace SL.ChatLinks.UI.Tabs.Items.Services;

public sealed class ItemSearch(ChatLinksContext context)
{
    private static readonly Regex ChatLinkPattern = new(@"^\[&[A-Za-z0-9+/=]+\]$", RegexOptions.Compiled);

    private readonly IQueryable<Item> _items = context.Items.AsNoTracking();

    public IAsyncEnumerable<Item> NewItems(int limit)
    {
        return _items
            .OrderByDescending(item => item.Id)
            .Take(limit)
            .AsAsyncEnumerable();
    }

    public IAsyncEnumerable<T> OfType<T>() where T : Item
    {
        return _items.OfType<T>()
            .AsAsyncEnumerable();
    }

    public async IAsyncEnumerable<Item> Search(string query, int limit,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (ChatLinkPattern.IsMatch(query))
        {
            ItemLink chatLink = ItemLink.Parse(query);
            await foreach (Item item in SearchByChatLink(chatLink, cancellationToken))
            {
                yield return item;
            }
        }
        else
        {
            await foreach (Item? item in context.Items.FromSqlInterpolated(
                $"""
                SELECT * FROM Items
                WHERE Name LIKE '%' || {query} || '%'
                ORDER BY LevenshteinDistance({query}, Name)
                """)
                .AsNoTracking()
                .Take(limit)
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken))
            {
                yield return item;
            }
        }
    }

    private async IAsyncEnumerable<Item> SearchByChatLink(ItemLink link,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Item item = await _items.SingleOrDefaultAsync(row => row.Id == link.ItemId, cancellationToken);
        if (item is null)
        {
            yield break;
        }

        yield return item;

        HashSet<int> relatedItems = new();
        if (link.SuffixItemId.HasValue)
        {
            relatedItems.Add(link.SuffixItemId.Value);
        }

        if (link.SecondarySuffixItemId.HasValue)
        {
            relatedItems.Add(link.SecondarySuffixItemId.Value);
        }

        switch (item)
        {
            case Weapon weapon:
                if (weapon.SuffixItemId.HasValue)
                {
                    relatedItems.Add(weapon.SuffixItemId.Value);
                }

                if (weapon.SecondarySuffixItemId.HasValue)
                {
                    relatedItems.Add(weapon.SecondarySuffixItemId.Value);
                }

                foreach (InfusionSlot? slot in weapon.InfusionSlots)
                {
                    if (slot.ItemId.HasValue)
                    {
                        relatedItems.Add(slot.ItemId.Value);
                    }
                }

                break;

            case Armor armor:
                if (armor.SuffixItemId.HasValue)
                {
                    relatedItems.Add(armor.SuffixItemId.Value);
                }

                foreach (InfusionSlot? slot in armor.InfusionSlots)
                {
                    if (slot.ItemId.HasValue)
                    {
                        relatedItems.Add(slot.ItemId.Value);
                    }
                }

                break;
            case Backpack back:
                if (back.SuffixItemId.HasValue)
                {
                    relatedItems.Add(back.SuffixItemId.Value);
                }

                foreach (InfusionSlot? slot in back.InfusionSlots)
                {
                    if (slot.ItemId.HasValue)
                    {
                        relatedItems.Add(slot.ItemId.Value);
                    }
                }

                foreach (InfusionSlotUpgradeSource? source in back.UpgradesFrom)
                {
                    relatedItems.Add(source.ItemId);
                }

                foreach (InfusionSlotUpgradePath? upgrade in back.UpgradesInto)
                {
                    relatedItems.Add(upgrade.ItemId);
                }

                break;

            case Trinket trinket:
                if (trinket.SuffixItemId.HasValue)
                {
                    relatedItems.Add(trinket.SuffixItemId.Value);
                }

                foreach (InfusionSlot? slot in trinket.InfusionSlots)
                {
                    if (slot.ItemId.HasValue)
                    {
                        relatedItems.Add(slot.ItemId.Value);
                    }
                }

                break;
            case CraftingMaterial material:

                foreach (InfusionSlotUpgradePath? upgrade in material.UpgradesInto)
                {
                    relatedItems.Add(upgrade.ItemId);
                }

                break;
        }

        await foreach (Item? relatedItem in _items
                           .Where(i => relatedItems.Contains(i.Id))
                           .AsAsyncEnumerable()
                           .WithCancellation(cancellationToken))
        {
            yield return relatedItem;
        }
    }

    private static int LevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a))
        {
            return string.IsNullOrEmpty(b) ? 0 : b.Length;
        }

        if (string.IsNullOrEmpty(b))
        {
            return a.Length;
        }

        int[,] costs = new int[a.Length + 1, b.Length + 1];
        for (int i = 0; i <= a.Length; i++) { costs[i, 0] = i; }

        for (int j = 0; j <= b.Length; j++) { costs[0, j] = j; }

        for (int i = 1; i <= a.Length; i++)
        {
            for (int j = 1; j <= b.Length; j++)
            {
                int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                costs[i, j] = Math.Min(Math.Min(costs[i - 1, j] + 1, costs[i, j - 1] + 1), costs[i - 1, j - 1] + cost);
            }
        }

        return costs[a.Length, b.Length];
    }
}