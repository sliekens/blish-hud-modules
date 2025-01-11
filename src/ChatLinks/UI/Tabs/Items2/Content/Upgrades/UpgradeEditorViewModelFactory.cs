using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common.Controls.Items.Services;
using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items2.Content.Upgrades;

public sealed class UpgradeEditorViewModelFactory(
    ItemIcons icons,
    Customizer customizer,
    UpgradeComponentListViewModelFactory upgradeComponentListViewModelFactory,
    ItemTooltipViewModelFactory itemTooltipViewModelFactory)
{
    public UpgradeEditorViewModel Create(Item targetItem, UpgradeSlotType slotType, int? defaultUpgradeComponentId)
    {
        UpgradeComponent? defaultUpgradeComponent = null;
        if (defaultUpgradeComponentId.HasValue)
        {
            customizer.UpgradeComponents.TryGetValue(defaultUpgradeComponentId.Value, out defaultUpgradeComponent);
        }

        var upgradeSlotViewModel = new UpgradeSlotViewModel(slotType, icons, itemTooltipViewModelFactory)
        {
            DefaultUpgradeComponent = defaultUpgradeComponent
        };

        return new UpgradeEditorViewModel(upgradeSlotViewModel, upgradeComponentListViewModelFactory, targetItem);
    }
}