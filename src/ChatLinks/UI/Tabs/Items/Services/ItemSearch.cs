using System.Runtime.CompilerServices;
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

    public IAsyncEnumerable<Item> NewItems(int limit, int offset)
    {
        return _items
            .OrderByDescending(item => item.Id)
            .Skip(offset)
            .Take(limit)
            .AsAsyncEnumerable();
    }

    public IAsyncEnumerable<T> OfType<T>() where T : Item
    {
        return _items.OfType<T>()
            .AsAsyncEnumerable();
    }

    public async IAsyncEnumerable<Item> Search(string query, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (ChatLinkPattern.IsMatch(query))
        {
            var chatLink = ItemLink.Parse(query);
            await foreach (Item item in SearchByChatLink(chatLink, cancellationToken))
            {
                yield return item;
            }
        }
        else
        {
            await foreach (var item in _items
               .Where(i => i.Name.ToLower().Contains(query.ToLowerInvariant()))
               .AsAsyncEnumerable()
               .WithCancellation(cancellationToken))
            {
                yield return item;
            }
        }
    }

    private async IAsyncEnumerable<Item> SearchByChatLink(ItemLink link, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        Item item = await _items.SingleOrDefaultAsync(row => row.Id == link.ItemId, cancellationToken);
        if (item is null)
        {
            yield break;
        }

        yield return item;

        var relatedItems = new HashSet<int>();
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

                foreach (var slot in weapon.InfusionSlots)
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

                foreach (var slot in armor.InfusionSlots)
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

                foreach (var slot in back.InfusionSlots)
                {
                    if (slot.ItemId.HasValue)
                    {
                        relatedItems.Add(slot.ItemId.Value);
                    }
                }

                foreach (var source in back.UpgradesFrom)
                {
                    relatedItems.Add(source.ItemId);
                }

                foreach (var upgrade in back.UpgradesInto)
                {
                    relatedItems.Add(upgrade.ItemId);
                }

                break;

            case Trinket trinket:
                if (trinket.SuffixItemId.HasValue)
                {
                    relatedItems.Add(trinket.SuffixItemId.Value);
                }

                foreach (var slot in trinket.InfusionSlots)
                {
                    if (slot.ItemId.HasValue)
                    {
                        relatedItems.Add(slot.ItemId.Value);
                    }
                }

                break;
            case CraftingMaterial material:

                foreach (var upgrade in material.UpgradesInto)
                {
                    relatedItems.Add(upgrade.ItemId);
                }

                break;

        }

        await foreach (var relatedItem in _items
            .Where(i => relatedItems.Contains(i.Id))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken))
        {
            yield return relatedItem;
        }
    }
}