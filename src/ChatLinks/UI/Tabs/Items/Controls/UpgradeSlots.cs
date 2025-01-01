using Blish_HUD.Controls;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.Common;
using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public sealed class UpgradeSlots : FlowPanel
{
    public UpgradeSlotsViewModel ViewModel { get; }

    private readonly List<(UpgradeSlot Slot, UpgradeComponentsList Options)> _slots = [];

    public UpgradeSlots(Item item, IReadOnlyDictionary<int, UpgradeComponent> upgrades)
    {
        ViewModel = ServiceLocator.Resolve<UpgradeSlotsViewModel>();
        ViewModel.Item = item;
        ViewModel.UpgradeComponents = upgrades;
        ViewModel.Initialize();

        FlowDirection = ControlFlowDirection.SingleTopToBottom;
        ControlPadding = new Vector2(20);

        foreach (var slotModel in ViewModel.Slots())
        {
            var slot = new UpgradeSlot(slotModel)
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize
            };

            var options = slotModel.Type switch
            {
                UpgradeSlotType.Default => ViewModel.UpgradeOptions,
                UpgradeSlotType.Infusion => ViewModel.InfusionOptions,
                UpgradeSlotType.Enrichment => ViewModel.EnrichmentOptions,
                _ => []
            } ?? [];

            var list = new UpgradeComponentsList(options)
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize
            };

            _slots.Add((slot, list));

            slot.ViewModel.Customized += () =>
            {
                if (list.Parent is null)
                {
                    ShowOptions(list);
                }
                else
                {
                    list.Parent = null;
                }
            };

            slot.ViewModel.Cleared += () =>
            {
                slotModel.SelectedUpgradeComponent = null;
                ViewModel.OnUpgradeChanged();
            };

            list.ViewModel.Selected += component =>
            {
                slotModel.SelectedUpgradeComponent = slotModel.SelectedUpgradeComponent == component
                    ? null
                    : component;
                slot.ViewModel.SelectedUpgradeComponent = slotModel.SelectedUpgradeComponent;
                ViewModel.OnUpgradeChanged();
            };
        }
    }

    private void ShowOptions(UpgradeComponentsList options)
    {
        foreach (var (slot, list) in _slots)
        {
            slot.Parent = null;
            slot.Parent = this;
            list.Parent = null;
            if (list == options)
            {
                list.Parent = this;
            }
        }
    }

}