using Blish_HUD.Controls;

using Microsoft.Xna.Framework;

using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public sealed class UpgradeSlots : FlowPanel
{
    public UpgradeSlotsViewModel ViewModel { get; }

    private readonly List<(UpgradeSlot Slot, UpgradeComponentsList Options)> _slots = [];

    public UpgradeSlots(UpgradeSlotsViewModel vm)
    {
        ViewModel = vm;
        FlowDirection = ControlFlowDirection.SingleTopToBottom;
        ControlPadding = new Vector2(20);

        foreach (var (slotViewModel, optionsViewModel) in vm.UpgradeSlots.Concat(vm.InfusionSlots))
        {
            var slot = new UpgradeSlot(slotViewModel)
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize
            };

            var options = new UpgradeComponentsList(optionsViewModel)
            {
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize
            };

            _slots.Add((slot, options));

            slotViewModel.Customized += () =>
            {
                if (options.Parent is null)
                {
                    ShowOptions(options);
                }
                else
                {
                    options.Parent = null;
                }
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