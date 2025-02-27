using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using GuildWars2.Hero.Achievements.Categories;

namespace SL.ChatLinks.UI.Tabs.Achievements;

internal sealed class AchievementsTabView : View, IDisposable
{
    private readonly ViewContainer _selectedCategoryView = new()
    {
        CanScroll = true
    };

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
            HeightSizingMode = SizingMode.Fill,
            Title = "Categories",
            CanScroll = true,
            ShowBorder = true
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

        _selectedCategoryView.Parent = buildPanel;
        _selectedCategoryView.Left = categoriesPanel.Right + 9;
        _selectedCategoryView.Width = 650;
        _selectedCategoryView.HeightSizingMode = SizingMode.Fill;

        ViewModel.PropertyChanged += (sender, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(ViewModel.HeaderText):
                    _selectedCategoryView.Title = ViewModel.HeaderText;
                    break;
                case nameof(ViewModel.HeaderIcon):
                    _selectedCategoryView.Icon = ViewModel.HeaderIcon;
                    break;
                case nameof(ViewModel.Achievements):
                    _selectedCategoryView.Show(new AchievementsListView(ViewModel.Achievements));
                    break;
            }
        };
    }

    public void Dispose()
    {
        _selectedCategoryView.Dispose();
        ViewModel.Dispose();
    }
}
