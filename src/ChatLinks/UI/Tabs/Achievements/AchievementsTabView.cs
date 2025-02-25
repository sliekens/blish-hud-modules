using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace SL.ChatLinks.UI.Tabs.Achievements;

internal sealed class AchievementsTabView : View, IDisposable
{
    public AchievementsTabViewModel ViewModel { get; }

    public AchievementsTabView(AchievementsTabViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    protected override void Build(Container buildPanel)
    {
        _ = new Label
        {
            Parent = buildPanel,
            Text = "Achievements Tab View works",
            AutoSizeWidth = true
        };
    }

    public void Dispose()
    {
        ViewModel.Dispose();
    }
}
