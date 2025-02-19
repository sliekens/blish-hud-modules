using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.Common.Controls;
using SL.Common.ModelBinding;

using Container = Blish_HUD.Controls.Container;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ItemsTabView : View, IDisposable
{
    public ItemsTabViewModel ViewModel { get; }

    private readonly FlowPanel _layout;

    private readonly ViewContainer _editor;

    public ItemsTabView(ItemsTabViewModel viewModel)
    {
        ThrowHelper.ThrowIfNull(viewModel);
        ViewModel = viewModel;
        ViewModel.Initialize();

        _layout = new FlowPanel
        {
            FlowDirection = ControlFlowDirection.SingleLeftToRight,
            WidthSizingMode = SizingMode.Fill,
            HeightSizingMode = SizingMode.Fill
        };

        FlowPanel searchLayout = new()
        {
            Parent = _layout,
            FlowDirection = ControlFlowDirection.SingleTopToBottom,
            Width = 400,
            HeightSizingMode = SizingMode.Fill
        };

        Panel searchBoxPanel = new()
        {
            Parent = searchLayout,
            WidthSizingMode = SizingMode.Fill,
            HeightSizingMode = SizingMode.AutoSize,
        };

        TextBox searchBox = new()
        {
            Parent = searchBoxPanel,
            Width = 400,
            PlaceholderText = viewModel.SearchPlaceholderText
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

        LoadingSpinner loadingSpinner = new()
        {
            Parent = searchBoxPanel,
            Size = new Point(searchBox.Height),
            Right = searchBox.Right
        };

        ItemsList searchResults = new()
        {
            Parent = searchLayout,
            WidthSizingMode = SizingMode.Standard,
            Width = 400,
            HeightSizingMode = SizingMode.Fill
        };

        searchResults.SetEntries(ViewModel.SearchResults);

        searchResults.SelectionChanged += SelectionChanged;

        _editor = new ViewContainer
        {
            Parent = _layout,
            Width = 450,
            HeightSizingMode = SizingMode.Fill,
            FadeView = true
        };

        _ = Binder.Bind(ViewModel, vm => vm.SearchText, searchBox);
        _ = Binder.Bind(ViewModel, vm => vm.Searching, loadingSpinner);
        _ = Binder.Bind(ViewModel, vm => vm.ResultText, searchLayout.Children.OfType<Scrollbar>().Single(), ctl => ctl.BasicTooltipText);

        viewModel.PropertyChanged += (_, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(ViewModel.SearchPlaceholderText):
                    searchBox.PlaceholderText = ViewModel.SearchPlaceholderText;
                    break;

                case nameof(ViewModel.SearchResults):
                    searchResults.SetEntries(ViewModel.SearchResults);
                    break;
                default:
                    break;
            }
        };
    }

    private void SelectionChanged(object sender, ListBoxSelectionChangedEventArgs<ItemsListViewModel> args)
    {
        if (args.AddedItems is [{ Data: { } listItem }])
        {
            ViewModel.SelectedItem = listItem.Item;
            ChatLinkEditorView view = new(ViewModel.CreateChatLinkEditorViewModel(listItem.Item));
            _editor.Show(view);
        }
        else
        {
            ViewModel.SelectedItem = null;
            _editor.Clear();
        }
    }

    protected override async Task<bool> Load(IProgress<string> progress)
    {
        await ViewModel.LoadAsync().ConfigureAwait(false);
        return true;
    }

    protected override void Build(Container buildPanel)
    {
        _layout.Parent = buildPanel;
    }

    protected override void Unload()
    {
        Dispose();
    }

    private void SearchTextChanged(object sender, EventArgs e)
    {
        ViewModel.SearchCommand.Execute(null);
    }

    private void SearchEnterPressed(object sender, EventArgs e)
    {
        ViewModel.SearchCommand.Execute(null);
    }

    public void Dispose()
    {
        _layout.Dispose();
        _editor.Dispose();
        ViewModel.Dispose();
    }
}
