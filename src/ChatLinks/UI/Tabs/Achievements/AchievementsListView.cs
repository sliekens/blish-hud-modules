using System.Collections.ObjectModel;

using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace SL.ChatLinks.UI.Tabs.Achievements;

public sealed class AchievementsListView(ObservableCollection<AchievementTileViewModel> achievements) : View, IDisposable
{
    private readonly FlowPanel _achievementsPanel = new();

    protected override void Build(Container buildPanel)
    {
        _achievementsPanel.Parent = buildPanel;
        _achievementsPanel.FlowDirection = ControlFlowDirection.LeftToRight;
        _achievementsPanel.WidthSizingMode = SizingMode.Fill;
        _achievementsPanel.HeightSizingMode = SizingMode.AutoSize;
        _achievementsPanel.ControlPadding = new(9, 7);
        _achievementsPanel.OuterControlPadding = new(0, 12);

        foreach (AchievementTileViewModel achievement in achievements)
        {
            _ = new AchievementTile(achievement)
            {
                Parent = _achievementsPanel
            };
        }
    }

    protected override void Unload()
    {
        Dispose();
    }

    public void Dispose()
    {
        while (_achievementsPanel.Children.Count > 0)
        {
            _achievementsPanel.Children[0].Dispose();
        }

        _achievementsPanel.Dispose();
    }
}
