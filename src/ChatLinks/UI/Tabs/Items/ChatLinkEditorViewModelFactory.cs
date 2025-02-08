using GuildWars2.Items;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using SL.ChatLinks.Storage;
using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.ChatLinks.UI.Tabs.Items.Upgrades;
using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ChatLinkEditorViewModelFactory(
    ItemTooltipViewModelFactory itemTooltipViewModelFactory,
    UpgradeEditorViewModelFactory upgradeEditorViewModelFactory,
    IOptionsMonitor<ChatLinkOptions> options,
    IStringLocalizer<ChatLinkEditor> localizer,
    ItemIcons icons,
    Customizer customizer,
    IClipBoard clipboard,
    IEventAggregator eventAggregator,
    IDbContextFactory contextFactory
)
{
    public ChatLinkEditorViewModel Create(Item item)
    {
        return new ChatLinkEditorViewModel(
            options,
            localizer,
            eventAggregator,
            contextFactory,
            itemTooltipViewModelFactory,
            upgradeEditorViewModelFactory,
            icons,
            customizer,
            clipboard,
            item
        );
    }
}