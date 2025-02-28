using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using Microsoft.Xna.Framework;

using SL.Common.Controls;


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

        FormattedLabel requirement = new FormattedLabelBuilder()
            .SetWidth(_layout.Width - 10)
            .AutoSizeHeight()
            .Wrap()
            .AddMarkup(ViewModel.Requirement)
            .Build();

        requirement.Parent = _layout;

        FormattedLabel description = new FormattedLabelBuilder()
            .SetWidth(_layout.Width - 10)
            .AutoSizeHeight()
            .Wrap()
            .AddMarkup(ViewModel.Description, part =>
            {
                _ = part.SetFontSize(ContentService.FontSize.Size16)
                    .MakeItalic();
            }, Gray)
            .Build();

        description.Parent = _layout;

        _layout.Parent = buildPanel;
    }

    public void Dispose()
    {
        _layout.Dispose();
        GC.SuppressFinalize(this);
    }
}
