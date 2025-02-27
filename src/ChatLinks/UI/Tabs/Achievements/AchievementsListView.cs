using System.Collections.ObjectModel;

using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using GuildWars2.Hero.Achievements;

namespace SL.ChatLinks.UI.Tabs.Achievements;

public sealed class AchievementsListView(ObservableCollection<Achievement> achievements) : View, IDisposable
{
    private readonly FlowPanel _achievementsPanel = new();

    protected override Task<bool> Load(IProgress<string> progress)
    {
        return base.Load(progress);
    }

    protected override void Build(Container buildPanel)
    {
        _achievementsPanel.Parent = buildPanel;
        _achievementsPanel.FlowDirection = ControlFlowDirection.LeftToRight;
        _achievementsPanel.WidthSizingMode = SizingMode.Fill;
        _achievementsPanel.HeightSizingMode = SizingMode.AutoSize;
        _achievementsPanel.ControlPadding = new(9, 7);

        foreach (Achievement achievement in achievements)
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
        _achievementsPanel.Dispose();
    }
}
