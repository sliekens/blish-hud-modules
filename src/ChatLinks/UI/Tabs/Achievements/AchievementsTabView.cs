using System.Collections.ObjectModel;

using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using GuildWars2.Hero.Achievements;
using GuildWars2.Hero.Achievements.Categories;

namespace SL.ChatLinks.UI.Tabs.Achievements;

internal sealed class AchievementsTabView : View, IDisposable
{
    private FlowPanel _achievementsPanel = new();

    public AchievementsTabViewModel ViewModel { get; }

    public AchievementsTabView(AchievementsTabViewModel viewModel)
    {
        ThrowHelper.ThrowIfNull(viewModel);
        ViewModel = viewModel;
    }

    protected override Task<bool> Load(IProgress<string> progress)
    {
        return ViewModel.Load();
    }

    protected override void Build(Container buildPanel)
    {
        Panel categoriesPanel = new()
        {
            Parent = buildPanel,
            WidthSizingMode = SizingMode.AutoSize,
            HeightSizingMode = SizingMode.AutoSize,
            Title = "Categories"
        };

        Menu menu = new()
        {
            Parent = categoriesPanel,
            Size = Panel.MenuStandard.Size
        };

        foreach (AchievementGroupMenuItem menuItem in ViewModel.MenuItems)
        {
            MenuItem groupMenuItem = menu.AddMenuItem(menuItem.Group.Name);
            groupMenuItem.BasicTooltipText = menuItem.Group.Description;

            foreach (AchievementCategory category in menuItem.Categories)
            {
                AsyncTexture2D icon = GameService.Content.GetRenderServiceTexture(category.IconHref)
                    .Duplicate();
                MenuItem categoryItem = new(category.Name, icon)
                {
                    Parent = groupMenuItem,
                    BasicTooltipText = category.Description
                };

                categoryItem.ItemSelected += (sender, args) =>
                {
                    _ = Task.Run(() => ViewModel.SelectCategory(category));
                };
            }
        }

        _achievementsPanel.Parent = buildPanel;
        _achievementsPanel.Left = categoriesPanel.Right;
        _achievementsPanel.Width = 600;
        _achievementsPanel.Height = 600;
        _achievementsPanel.Title = "Achievements";

        ViewModel.PropertyChanged += (sender, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(ViewModel.Achievements):
                    ShowAchievements(ViewModel.Achievements);
                    break;
            }
        };
    }

    private void ShowAchievements(ObservableCollection<Achievement> achievements)
    {
        foreach (Achievement achievement in achievements)
        {
            DetailsButton button = new()
            {
                Parent = _achievementsPanel,
                Text = achievement.Name
            };

            if (!string.IsNullOrEmpty(achievement.Description))
            {
                button.BasicTooltipText = achievement.Description;
            }

            if (!string.IsNullOrEmpty(achievement.IconHref))
            {
                button.Icon = GameService.Content.GetRenderServiceTexture(achievement.IconHref);
            }

            _ = new TextBox
            {
                Parent = button,
                Width = 200,
                Text = achievement.GetChatLink().ToString()
            };
        }
    }

    public void Dispose()
    {
        ViewModel.Dispose();
    }
}
