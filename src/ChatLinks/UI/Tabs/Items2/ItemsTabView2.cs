using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using GuildWars2.Items;

using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items;
using SL.ChatLinks.UI.Tabs.Items2.Collections;
using SL.ChatLinks.UI.Tabs.Items2.Content;
using SL.Common.Controls;
using SL.Common.ModelBinding;

using Container = Blish_HUD.Controls.Container;

namespace SL.ChatLinks.UI.Tabs.Items2;

public class ItemsTabView2 : View
{
    public ItemsTabViewModel ViewModel { get; }

    private readonly TextBox _searchBox;

    private readonly LoadingSpinner _loadingSpinner;

    private readonly ItemsList _searchResults;

    private readonly ViewContainer _editor;

    public ItemsTabView2(ILogger<ItemsTabView> logger, ItemsTabViewModel viewModel)
    {
        ViewModel = viewModel;
        _searchBox = new TextBox { Width = 450, PlaceholderText = "Enter item name or chat link..." };

        _loadingSpinner = new LoadingSpinner { Size = new Point(_searchBox.Height), Right = _searchBox.Right };

        _searchResults = new ItemsList
        {
            WidthSizingMode = SizingMode.Standard,
            HeightSizingMode = SizingMode.Standard,
            Size = new Point(450, 500),
            Top = _searchBox.Bottom,
            Entries = ViewModel.SearchResults
        };

        _editor = new ViewContainer
        {
            Size = new Point(350, 500),
            Left = _searchResults.Right,
            FadeView = true
        };

        _searchResults.SelectionChanged += SelectionChanged;
    }

    private void SelectionChanged(ListBox<ItemsListViewModel> sender, ListBoxSelectionChangedEventArgs<ItemsListViewModel> args)
    {
        if (args.AddedItems is [{ Data: { } listItem }])
        {
            _editor.Show(new ChatLinkEditor(ViewModel.CreateChatLinkEditorViewModel(listItem.Item)));
        }
        else
        {
            _editor.Clear();
        }
    }

    protected override async Task<bool> Load(IProgress<string> progress)
    {
        await ViewModel.LoadAsync();
        return true;
    }

    protected override void Build(Container buildPanel)
    {
        _searchBox.Parent = buildPanel;
        _loadingSpinner.Parent = buildPanel;
        _searchResults.Parent = buildPanel;
        _editor.Parent = buildPanel;

        Binder.Bind(ViewModel, vm => vm.SearchText, _searchBox);
        Binder.Bind(ViewModel, vm => vm.Searching, _loadingSpinner);

        _searchBox.TextChanged += SearchTextChanged;
        _searchBox.EnterPressed += SearchEnterPressed;
        _searchBox.InputFocusChanged += SearchInputFocusChanged;
    }

    protected override void Unload()
    {
        _searchBox.TextChanged -= SearchEnterPressed;
        _searchBox.EnterPressed -= SearchEnterPressed;
        _searchBox.InputFocusChanged -= SearchInputFocusChanged;
    }

    private void SearchTextChanged(object sender, EventArgs e)
    {
        ViewModel.SearchCommand.Execute(null);
    }

    private void SearchEnterPressed(object sender, EventArgs e)
    {
        ViewModel.SearchCommand.Execute(null);
    }

    private void SearchInputFocusChanged(object sender, ValueEventArgs<bool> args)
    {
        if (args.Value)
        {
            _searchBox.SelectionStart = 0;
            _searchBox.SelectionEnd = _searchBox.Length;
        }
        else
        {
            _searchBox.SelectionStart = _searchBox.SelectionEnd;
        }
    }
}