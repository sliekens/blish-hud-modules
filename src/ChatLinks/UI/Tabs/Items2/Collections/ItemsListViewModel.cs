using Blish_HUD.Content;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common;
using SL.Common.Controls.Items.Services;

namespace SL.ChatLinks.UI.Tabs.Items2.Collections;

public sealed class ItemsListViewModel(
    ItemIcons icons,
    Item item,
    ItemTooltipViewModelFactory tooltipViewModelFactory,
    bool isSelected
) : ViewModel
{
    private bool _isSelected = isSelected;

    public Item Item { get; } = item ?? throw new ArgumentNullException(nameof(item));

    public Color Color { get; } = ItemColors.Rarity(item.Rarity);

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    public AsyncTexture2D? GetIcon()
    {
        return icons.GetIcon(item);
    }

    public ItemTooltipViewModel CreateTooltipViewModel()
    {
        return tooltipViewModelFactory.Create(item);
    }
}