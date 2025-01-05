using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using Microsoft.Extensions.Logging;

using SL.ChatLinks.UI.Tabs.Items;
using SL.Common.ModelBinding;

using Container = Blish_HUD.Controls.Container;

namespace SL.ChatLinks.UI.Tabs.Items2;

public class ItemsTabView2(ILogger<ItemsTabView> logger, ItemsTabViewModel viewModel) : View
{
    public ItemsTabViewModel ViewModel { get; } = viewModel;

    private readonly TextBox _searchBox = new()
    {
        Width = 450,
        PlaceholderText = "Enter item name or chat link..."
    };

    protected override async Task<bool> Load(IProgress<string> progress)
    {
        await ViewModel.LoadAsync();
        return true;
    }

    protected override void Build(Container buildPanel)
    {
        ViewModel.EnsureLoaded();
        _searchBox.Parent = buildPanel;

        Binder.Bind(ViewModel, vm => vm.SearchText, _searchBox);

        _searchBox.TextChanged += SearchTextChanged;
        _searchBox.EnterPressed += SearchEnterPressed;
        _searchBox.InputFocusChanged += SearchInputFocusChanged;
    }

    protected override void Unload()
    {
        ViewModel.Unload();
        _searchBox.TextChanged -= SearchEnterPressed;
        _searchBox.EnterPressed -= SearchEnterPressed;
        _searchBox.InputFocusChanged -= SearchInputFocusChanged;
    }

    private void SearchTextChanged(object sender, EventArgs e)
    {
        if (ViewModel.SearchCommand.CanExecute(null!))
        {
            ViewModel.SearchCommand.Execute(null!);
        }
    }

    private void SearchEnterPressed(object sender, EventArgs e)
    {
        if (ViewModel.SearchCommand.CanExecute(null!))
        {
            ViewModel.SearchCommand.Execute(null!);
        }
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