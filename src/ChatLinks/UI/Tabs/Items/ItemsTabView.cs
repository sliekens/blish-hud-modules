using System.Diagnostics;

using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.Common.ModelBinding;

using Container = Blish_HUD.Controls.Container;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ItemsTabView(
    ItemsTabViewModel viewModel
) : View, IDisposable
{
    private Panel? _sidePanel;

    private Menu? _sidebar;

    private Container? _layout;

    private Panel? _contentPanel;

    private ItemsList? _searchResults;

    private Container? _selection;

    protected override async Task<bool> Load(IProgress<string> progress)
    {
        await viewModel.Load().ConfigureAwait(false);
        return true;
    }

    protected override void Build(Container buildPanel)
    {
        _layout = buildPanel;

        Panel searchBoxPanel = new()
        {
            Parent = _layout,
            Left = 4,
            WidthSizingMode = SizingMode.AutoSize,
            HeightSizingMode = SizingMode.AutoSize,
        };

        TextBox searchBox = new()
        {
            Parent = searchBoxPanel,
            Width = Panel.MenuStandard.Size.X,
            PlaceholderText = viewModel.SearchPlaceholder
        };

        _ = Binder.Bind(viewModel, vm => vm.SearchPlaceholder, searchBox, ctl => ctl.PlaceholderText);
        _ = Binder.Bind(viewModel, vm => vm.SearchText, searchBox);

        searchBox.TextChanged += (sender, args) =>
        {
            viewModel.SearchCommand.Execute();
        };

        searchBox.EnterPressed += (sender, args) =>
        {
            viewModel.SearchCommand.Execute();
        };

        searchBox.InputFocusChanged += (sender, args) =>
        {
            if (args.Value)
            {
                searchBox.SelectionStart = 0;
                searchBox.SelectionEnd = searchBox.Length;
            }
            else
            {
                searchBox.SelectionStart = searchBox.SelectionEnd;
            }
        };

        LoadingSpinner loadingSpinner = new()
        {
            Parent = searchBoxPanel,
            Size = new Point(searchBox.Height),
            Right = searchBox.Right
        };

        _ = Binder.Bind(viewModel, vm => vm.Searching, loadingSpinner);

        _sidePanel = new Panel
        {
            Parent = buildPanel,
            Top = searchBox.Height + 9,
            Width = Panel.MenuStandard.Size.X,
            HeightSizingMode = SizingMode.Fill,
            CanScroll = true
        };

        _sidebar = new Menu
        {
            Parent = _sidePanel,
            Size = Panel.MenuStandard.Size,
            CanSelect = true
        };

        WireUp(_sidebar, viewModel.MenuItems);

        _contentPanel = new Panel
        {
            Parent = _layout,
            Left = _sidePanel.Right + Control.ControlStandard.ControlOffset.X,
            WidthSizingMode = SizingMode.Fill,
            HeightSizingMode = SizingMode.Fill
        };

        Binder.Bind(viewModel, vm => vm.ContentTitle, _contentPanel, ctl => ctl.Title);
        Binder.Bind(viewModel, vm => vm.ContentIcon, _contentPanel, ctl => ctl.Icon);

        _contentPanel.Click += (sender, args) =>
        {
            // Check if title bar is clicked
            if (args.MousePosition.Y - _contentPanel.AbsoluteBounds.Y <= 40)
            {
                viewModel.BackCommand.Execute();
            }
        };

        _searchResults = new ItemsList
        {
            Parent = _contentPanel,
            WidthSizingMode = SizingMode.Fill,
            HeightSizingMode = SizingMode.Fill
        };

        _searchResults.SetEntries(viewModel.SearchResults);
        _searchResults.SelectionChanged += (sender, args) =>
        {
            if (args.AddedItems.Count == 1)
            {
                // TODO: fix weird view model usage
                ItemsListViewModel selected = args.AddedItems[0].Data;
                viewModel.SelectItemCommand.Execute(selected.Item);
            }
        };

        _ = Binder.Bind(viewModel, vm => vm.ResultText, _contentPanel.Children.OfType<Scrollbar>().Single(), ctl => ctl.BasicTooltipText);

        viewModel.PropertyChanged += (_, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(viewModel.MenuItems):
                    ReloadMenuItems();
                    break;

                case nameof(viewModel.SearchResults):
                    ShowSearchResults();
                    break;
                case nameof(viewModel.SelectedItem):
                    ShowItem(viewModel.SelectedItem);
                    break;

                case nameof(viewModel.ContentIcon):
                    _contentPanel.Invalidate();
                    break;

                default:
                    break;
            }
        };
    }

    private void ReloadMenuItems()
    {
        if (_sidebar is null) return;
        while (_sidebar.Children.Count > 0)
        {
            _sidebar.Children[0].Dispose();
        }

        WireUp(_sidebar, viewModel.MenuItems);
    }

    private void WireUp(Container parent, IList<ItemCategoryMenuItem> categories)
    {
        foreach (ItemCategoryMenuItem category in categories)
        {
            MenuItem menuItem = new()
            {
                Parent = parent,
                Text = category.Label
            };

            if (category.CanSelect)
            {
                menuItem.ItemSelected += (sender, args) =>
                {
                    if (category.Id == "recently_added")
                    {
                        viewModel.ShowRecentCommand.Execute();
                    }
                    else
                    {
                        viewModel.ShowCategoryCommand.Execute(new ItemsFilter
                        {
                            Category = category.Id,
                            Label = category.Label
                        });
                    }
                };
            }

            WireUp(menuItem, category.Subcategories);

            if (category.Id == viewModel.SelectedCategory)
            {
                menuItem.Select();
            }

            viewModel.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(viewModel.SelectedCategory):
                        if (viewModel.SelectedCategory == category.Id)
                        {
                            menuItem.Select();
                        }

                        break;
                }
            };
        }
    }

    private void ShowSearchResults()
    {
        Debug.Assert(_searchResults is not null);
        _selection?.Dispose();
        _selection = null;
        _searchResults!.SetEntries(viewModel.SearchResults);
        _searchResults.Parent = _contentPanel;
    }

    private void ShowItem(Item? item)
    {
        Debug.Assert(_searchResults is not null);
        if (item is null)
        {
            ShowSearchResults();
        }
        else
        {
            _searchResults!.Parent = null;

            ChatLinkEditor editor = new(viewModel.CreateChatLinkEditorViewModel(item))
            {
                Parent = _contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.Fill

            };

            _selection?.Dispose();
            _selection = editor;
        }
    }

    protected override void Unload()
    {
        Dispose();
    }

    public void Dispose()
    {
        _sidePanel?.Dispose();
        _sidebar?.Dispose();
        _contentPanel?.Dispose();
        _searchResults?.Dispose();
        _selection?.Dispose();
        viewModel.Dispose();
    }
}
