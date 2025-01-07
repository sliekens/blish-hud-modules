using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;

namespace SL.ChatLinks.UI.Tabs.Items2.Content;

public sealed class ChatLinkEditorViewModelFactory(
    ItemTooltipViewModelFactory itemTooltipViewModelFactory
)
{
    public ChatLinkEditorViewModel Create(Item item)
    {
        return new ChatLinkEditorViewModel(
            itemTooltipViewModelFactory,
            item
        );
    }
}