using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items2.Content.Upgrades;
using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common;
using SL.Common.Controls.Items.Services;

namespace SL.ChatLinks.UI.Tabs.Items2.Content;

public sealed class ChatLinkEditorViewModelFactory(
    ItemTooltipViewModelFactory itemTooltipViewModelFactory,
    UpgradeSlotViewModelFactory upgradeEditorViewModelFactory,
    ItemIcons icons,
    IClipBoard clipboard
)
{
    public ChatLinkEditorViewModel Create(Item item)
    {
        return new ChatLinkEditorViewModel(
            itemTooltipViewModelFactory,
            upgradeEditorViewModelFactory,
            icons,
            clipboard,
            item
        );
    }
}