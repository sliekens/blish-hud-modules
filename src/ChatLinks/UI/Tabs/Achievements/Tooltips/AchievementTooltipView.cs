using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using Microsoft.Xna.Framework;


namespace SL.ChatLinks.UI.Tabs.Achievements.Tooltips;

public sealed class AchievementTooltipView(AchievementTooltipViewModel viewModel) : View, ITooltipView, IDisposable
{
    private static readonly Color LightOrange = new(0xff, 0xcc, 0x77);

    private static readonly Color Gray = new(0x99, 0x99, 0x99);

    private readonly FlowPanel _layout = new()
    {
        FlowDirection = ControlFlowDirection.SingleTopToBottom,
        Width = 350,
        HeightSizingMode = SizingMode.AutoSize,
        ControlPadding = new(0, 5)
    };

    public AchievementTooltipViewModel ViewModel { get; } = viewModel;

    protected override void Unload()
    {
        Dispose();
    }

    protected override void Build(Container buildPanel)
    {
        Label name = new()
        {
            Parent = _layout,
            Text = ViewModel.Name,
            Font = GameService.Content.DefaultFont16,
            TextColor = LightOrange,
            WrapText = true,
            Width = _layout.Width,
            AutoSizeHeight = true,
        };

        Label requirement = new()
        {
            Parent = _layout,
            Text = ViewModel.Requirement,
            WrapText = true,
            Width = _layout.Width,
            AutoSizeHeight = true,
        };

        Label description = new()
        {
            Parent = _layout,
            Text = ViewModel.Description,
            TextColor = Gray,
            WrapText = true,
            Width = _layout.Width,
            AutoSizeHeight = true,
        };

        _layout.Parent = buildPanel;
    }

    public void Dispose()
    {
        _layout.Dispose();
        GC.SuppressFinalize(this);
    }
}
