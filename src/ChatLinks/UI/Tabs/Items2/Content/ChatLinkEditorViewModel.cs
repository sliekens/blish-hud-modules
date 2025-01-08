using Blish_HUD.Content;

using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common;
using SL.Common.Controls.Items.Services;

namespace SL.ChatLinks.UI.Tabs.Items2.Content;

public sealed class ChatLinkEditorViewModel(
    ItemTooltipViewModelFactory tooltipViewModelFactory,
    ItemIcons icons,
    Item item
) : ViewModel
{
    public Item Item { get; } = item;

    public ItemTooltipViewModel CreateTooltipViewModel()
    {
        return tooltipViewModelFactory.Create(Item);
    }

    public AsyncTexture2D? GetIcon()
    {
        return icons.GetIcon(Item);
    }
}