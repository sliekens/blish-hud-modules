using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items2.Content;

public sealed class ChatLinkEditorViewModel(
    ItemTooltipViewModelFactory tooltipViewModelFactory,
    Item item
)
    : ViewModel
{
    public Item Item { get; } = item;

    public ItemTooltipViewModel CreateTooltipViewModel()
    {
        return tooltipViewModelFactory.Create(Item);
    }
}