using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.ChatLinks.UI.Tabs.Items.Upgrades;
using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ChatLinkEditorViewModelFactory(
    ItemTooltipViewModelFactory itemTooltipViewModelFactory,
    UpgradeEditorViewModelFactory upgradeEditorViewModelFactory,
    ItemIcons icons,
    Customizer customizer,
    IClipBoard clipboard,
    IEventAggregator eventAggregator)
{
    public ChatLinkEditorViewModel Create(Item item)
    {
        return new ChatLinkEditorViewModel(
            eventAggregator,
            itemTooltipViewModelFactory,
            upgradeEditorViewModelFactory,
            icons,
            customizer,
            clipboard,
            item
        );
    }
}