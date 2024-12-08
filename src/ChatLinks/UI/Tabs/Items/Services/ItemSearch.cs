using System.Text.RegularExpressions;

using GuildWars2.Chat;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;

using SL.ChatLinks.Storage;

namespace SL.ChatLinks.UI.Tabs.Items.Services;

public sealed class ItemSearch(ChatLinksContext context)
{
    private static readonly Regex ChatLinkPattern = new(@"^\[&[A-Za-z0-9+/=]+\]$", RegexOptions.Compiled);

    public IAsyncEnumerable<Item> NewItems(int count)
    {
        return context.Items.OrderByDescending(item => item.Id)
            .Take(count)
            .AsAsyncEnumerable();
    }

    public async IAsyncEnumerable<Item> Search(string query, CancellationToken cancellationToken)
    {
        if (ChatLinkPattern.IsMatch(query))
        {
            ItemLink link = ItemLink.Parse(query);
            if (await context.Items.FindAsync(link.ItemId) is not { } item)
            {
                yield break;
            }

            yield return item;

            switch (item)
            {
                case Weapon weapon:
                    {
                        if (weapon.SuffixItemId.HasValue &&
                            await context.Items.FindAsync(weapon.SuffixItemId.Value) is { } suffixItem)
                        {
                            yield return suffixItem;
                        }

                        if (weapon.SecondarySuffixItemId.HasValue &&
                            await context.Items.FindAsync(weapon.SecondarySuffixItemId.Value) is
                            { } secondarySuffixItem)
                        {
                            yield return secondarySuffixItem;
                        }

                        break;
                    }
                case Armor armor:
                    {
                        if (armor.SuffixItemId.HasValue && await context.Items.FindAsync(armor.SuffixItemId.Value) is
                            { } suffixItem)
                        {
                            yield return suffixItem;
                        }

                        break;
                    }
                case Backpack back:
                    {
                        if (back.SuffixItemId.HasValue && await context.Items.FindAsync(back.SuffixItemId.Value) is
                            { } suffixItem)
                        {
                            yield return suffixItem;
                        }

                        break;
                    }
                case Trinket trinket:
                    {
                        if (trinket.SuffixItemId.HasValue &&
                            await context.Items.FindAsync(trinket.SuffixItemId.Value) is { } suffixItem)
                        {
                            yield return suffixItem;
                        }

                        break;
                    }
            }
        }
        else
        {
            await foreach (var item in context.Items
               .Where(i => i.Name.ToLower().Contains(query.ToLowerInvariant()))
               .AsAsyncEnumerable()
               .WithCancellation(cancellationToken))
            {
                yield return item;
            }
        }
    }

}