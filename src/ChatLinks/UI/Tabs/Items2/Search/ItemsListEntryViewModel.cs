using Blish_HUD.Content;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common;
using SL.Common.Controls.Items.Services;

namespace SL.ChatLinks.UI.Tabs.Items2.Search;

public sealed class ItemsListEntryViewModel(
    ItemIcons icons,
    Item item,
    ItemTooltipViewModelFactory tooltipViewModelFactory)
{
    public Item Item { get; } = item ?? throw new ArgumentNullException(nameof(item));

    public Color Color { get; } = ItemColors.Rarity(item.Rarity);

    public AsyncTexture2D? GetIcon()
    {
        return icons.GetIcon(item);
    }

    public ItemTooltipViewModel CreateTooltipViewModel()
    {
        return tooltipViewModelFactory.Create(item);
    }
}