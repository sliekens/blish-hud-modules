using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using GuildWars2.Hero.Achievements.Categories;

using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Achievements;

public sealed class AchievementsTabView : View, IDisposable
{
    private readonly TextBox _searchBox;

    private readonly Panel _categoriesPanel;

    private readonly Menu _sidebar;

    private readonly ViewContainer _selectedCategoryView;

    public AchievementsTabViewModel ViewModel { get; }

    private event EventHandler<EventArgs>? MenuItemExpanded;

    public AchievementsTabView(AchievementsTabViewModel viewModel)
    {
        ThrowHelper.ThrowIfNull(viewModel);
        ViewModel = viewModel;

        _searchBox = new()
        {
            Left = 4,
            Width = Panel.MenuStandard.Size.X
        };

        _ = Binder.Bind(viewModel, vm => vm.SearchPlaceholder, _searchBox, searchBox => searchBox.PlaceholderText);

        _categoriesPanel = new()
        {
            Top = _searchBox.Height + 9,
            WidthSizingMode = SizingMode.AutoSize,
            HeightSizingMode = SizingMode.Fill,
            CanScroll = true,
            ShowBorder = true
        };

        _ = Binder.Bind(viewModel, vm => vm.CategoriesTitle, _categoriesPanel, panel => panel.Title);

        _sidebar = new Menu
        {
            Parent = _categoriesPanel,
            Size = Panel.MenuStandard.Size,
            CanSelect = true
        };

        _selectedCategoryView = new()
        {
            CanScroll = true
        };

        viewModel.PropertyChanged += (sender, args) =>
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
                case nameof(ViewModel.MenuItems):
                    ReloadMenuItems();
                    break;
                default:
                    break;
            }
        };
    }

    protected override Task<bool> Load(IProgress<string> progress)
    {
        return ViewModel.Load();
    }

    protected override void Build(Container buildPanel)
    {
        _searchBox.Parent = buildPanel;
        _categoriesPanel.Parent = buildPanel;
        AddAchievementCategories();

        _selectedCategoryView.Parent = buildPanel;
        _selectedCategoryView.Left = _categoriesPanel.Right + 9;
        _selectedCategoryView.Width = 650;
        _selectedCategoryView.HeightSizingMode = SizingMode.Fill;

        _ = Binder.Bind(ViewModel, vm => vm.SearchText, _searchBox);
        _searchBox.TextChanged += SearchTextChanged;
        _searchBox.EnterPressed += SearchEnterPressed;
    }

    private void ReloadMenuItems()
    {
        MenuItemExpanded = null;
        while (_sidebar.Children.Count > 0)
        {
            _sidebar.Children[0].Dispose();
        }

        AddAchievementCategories();
    }

    private void AddAchievementCategories()
    {
        foreach (AchievementGroupMenuItem menuItem in ViewModel.MenuItems)
        {
            MenuItem groupMenuItem = _sidebar.AddMenuItem(menuItem.Group.Name);
            groupMenuItem.BasicTooltipText = menuItem.Group.Description;

            foreach (AchievementCategory category in menuItem.Categories)
            {
                AsyncTexture2D icon = ViewModel.GetIcon(category.IconUrl());

                MenuItem categoryItem = new(category.Name, icon)
                {
                    Parent = groupMenuItem,
                    BasicTooltipText = category.Description
                };

                categoryItem.ItemSelected += (sender, args) =>
                {
                    _ = Task.Run(() => ViewModel.SelectCategory(category));
                };

                if (category.Id == ViewModel.SelectedCategory?.Id)
                {
                    categoryItem.Select();
                }
            }

            groupMenuItem.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case "Expand":
                        MenuItemExpanded?.Invoke(sender, EventArgs.Empty);
                        break;
                    default:
                        break;
                }
            };

            MenuItemExpanded += (sender, args) =>
            {
                if (sender != groupMenuItem)
                {
                    groupMenuItem.Collapse();
                }
            };
        }
    }

    private void SearchTextChanged(object sender, EventArgs e)
    {
        _sidebar.SelectedMenuItem?.Deselect();
        ViewModel.SearchCommand.Execute(null);
    }

    private void SearchEnterPressed(object sender, EventArgs e)
    {
        _sidebar.SelectedMenuItem?.Deselect();
        ViewModel.SearchCommand.Execute(null);
    }

    public void Dispose()
    {
        _searchBox.Dispose();
        _categoriesPanel.Dispose();
        _sidebar.Dispose();
        _selectedCategoryView.Dispose();
        ViewModel.Dispose();
    }
}
