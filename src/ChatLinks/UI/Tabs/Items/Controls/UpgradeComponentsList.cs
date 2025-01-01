using Blish_HUD.Controls;

using GuildWars2.Items;

using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public sealed class UpgradeComponentsList : FlowPanel
{
    public UpgradeComponentsListViewModel ViewModel { get; }

    private readonly StandardButton _cancelButton;

    public UpgradeComponentsList(IEnumerable<UpgradeComponent> upgrades)
    {
        ViewModel = ServiceLocator.Resolve<UpgradeComponentsListViewModel>();
        ViewModel.Options = upgrades.ToList().AsReadOnly();
        FlowDirection = ControlFlowDirection.SingleTopToBottom;
        HeightSizingMode = SizingMode.Fill;

        _cancelButton = new StandardButton
        {
            Parent = this,
            Text = "Cancel"
        };

        _cancelButton.Click += (_, _) =>
        {
            Parent = null;
        };

        ViewModel.Selected += _ =>
        {
            Parent = null;
        };

        Initialize();
    }

    protected override void OnResized(ResizedEventArgs e)
    {
        _cancelButton.Width = e.CurrentSize.X;
        Parent?.Invalidate();
    }

    private void Initialize()
    {
        foreach (var group in ViewModel.GetOptions())
        {
            var groupPanel = new Panel
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Title = group.Key,
                CanCollapse = true,
                Collapsed = true
            };

            var list = new ItemsList(new Dictionary<int, UpgradeComponent>(0))
            {
                Parent = groupPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                CanScroll = false
            };

            list.SetOptions(group);
            list.OptionClicked += OptionClicked;

            groupPanel.Resized += (sender, args) =>
            {
                if (list.Height >= 300)
                {
                    list.HeightSizingMode = SizingMode.Standard;
                    list.Height = 300;
                    list.CanScroll = true;
                }
                else
                {
                    list.HeightSizingMode = SizingMode.AutoSize;
                    list.CanScroll = false;
                }
            };
        }
    }

    private void OptionClicked(object sender, Item e)
    {
        ViewModel.OnSelected((UpgradeComponent)e);
    }
}