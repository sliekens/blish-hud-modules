using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.ChatLinks.UI.Tabs.Items2.Upgrades;
using SL.Common;
using SL.Common.Controls.Items.Services;

namespace SL.ChatLinks.UI.Tabs.Items2;

public sealed class ChatLinkEditorViewModelFactory(
    ItemTooltipViewModelFactory itemTooltipViewModelFactory,
    UpgradeEditorViewModelFactory upgradeEditorViewModelFactory,
    ItemIcons icons,
    Customizer customizer,
    IClipBoard clipboard
)
{
    public ChatLinkEditorViewModel Create(Item item)
    {
        return new ChatLinkEditorViewModel(
            itemTooltipViewModelFactory,
            upgradeEditorViewModelFactory,
            icons,
            customizer,
            clipboard,
            item
        );
    }
}