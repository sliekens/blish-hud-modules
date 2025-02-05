using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.Common.Controls;
using SL.Common.ModelBinding;

using Container = Blish_HUD.Controls.Container;

namespace SL.ChatLinks.UI.Tabs.Items;

public class ItemsTabView : View
{
    public ItemsTabViewModel ViewModel { get; }

    private readonly FlowPanel _layout;

    private readonly ViewContainer _editor;

    public ItemsTabView(ItemsTabViewModel viewModel)
    {
        ViewModel = viewModel;
        ViewModel.Initialize();

        _layout = new FlowPanel
        {
            FlowDirection = ControlFlowDirection.SingleLeftToRight,
            WidthSizingMode = SizingMode.Fill,
            HeightSizingMode = SizingMode.Fill
        };

        var searchLayout = new FlowPanel
        {
            Parent = _layout,
            FlowDirection = ControlFlowDirection.SingleTopToBottom,
            Width = 400,
            HeightSizingMode = SizingMode.Fill
        };

        var searchBoxPanel = new Panel
        {
            Parent = searchLayout,
            WidthSizingMode = SizingMode.Fill,
            HeightSizingMode = SizingMode.AutoSize,
        };

        var searchBox = new TextBox
        {
            Parent = searchBoxPanel,
            Width = 400,
            PlaceholderText = "Enter item name or chat link..."
        };

        searchBox.TextChanged += SearchTextChanged;
        searchBox.EnterPressed += SearchEnterPressed;
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

        var loadingSpinner = new LoadingSpinner
        {
            Parent = searchBoxPanel,
            Size = new Point(searchBox.Height),
            Right = searchBox.Right
        };

        var searchResults = new ItemsList
        {
            Parent = searchLayout,
            WidthSizingMode = SizingMode.Standard,
            Width = 400,
            HeightSizingMode = SizingMode.Fill,
            Entries = ViewModel.SearchResults
        };

        searchResults.SelectionChanged += SelectionChanged;

        _editor = new ViewContainer
        {
            Parent = _layout,
            Width = 450,
            HeightSizingMode = SizingMode.Fill,
            FadeView = true
        };

        Binder.Bind(ViewModel, vm => vm.SearchText, searchBox);
        Binder.Bind(ViewModel, vm => vm.Searching, loadingSpinner);
        Binder.Bind(ViewModel, vm => vm.ResultText, searchLayout.Children.OfType<Scrollbar>().Single());
    }

    private void SelectionChanged(ListBox<ItemsListViewModel> sender, ListBoxSelectionChangedEventArgs<ItemsListViewModel> args)
    {
        if (args.AddedItems is [{ Data: { } listItem }])
        {
            _editor.Show(new ChatLinkEditorView(ViewModel.CreateChatLinkEditorViewModel(listItem.Item)));
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
        _layout.Parent = buildPanel;
    }

    protected override void Unload()
    {
        ViewModel.Dispose();
    }

    private void SearchTextChanged(object sender, EventArgs e)
    {
        ViewModel.SearchCommand.Execute(null);
    }

    private void SearchEnterPressed(object sender, EventArgs e)
    {
        ViewModel.SearchCommand.Execute(null);
    }
}