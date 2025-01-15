using GuildWars2.Items;

using Microsoft.Extensions.Options;

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
    IEventAggregator eventAggregator,
    IOptionsMonitor<ChatLinkOptions> options)
{
    public ChatLinkEditorViewModel Create(Item item)
    {
        return new ChatLinkEditorViewModel(
            options,
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