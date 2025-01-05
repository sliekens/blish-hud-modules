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

    private Container? _root;

    private TextBox? _searchBox;

    protected override async Task<bool> Load(IProgress<string> progress)
    {
        await ViewModel.LoadAsync();
        return true;
    }

    protected override void Build(Container buildPanel)
    {
        ViewModel.EnsureLoaded();
        _root = buildPanel;
        _searchBox = new TextBox
        {
            Parent = buildPanel,
            Width = 450,
            PlaceholderText = "Enter item name or chat link..."
        };

        Binder.Bind(ViewModel, vm => vm.SearchText, _searchBox);

        _searchBox.TextChanged += SearchTextChanged;
        _searchBox.EnterPressed += SearchEnterPressed;
        _searchBox.InputFocusChanged += (_, args) =>
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
        };
    }

    protected override void Unload()
    {
        ViewModel.Unload();
        if (_searchBox is not null)
        {
            _searchBox.TextChanged -= SearchEnterPressed;
        }
    }

    private async void SearchTextChanged(object sender, EventArgs e)
    {
        try
        {
            if (_searchBox is null)
            {
                return;
            }

            if (_searchBox.Focused)
            {
                await ViewModel.Search();
            }
        }
        catch (Exception reason)
        {
            logger.LogError(reason, "Failed to search for items");
            ScreenNotification.ShowNotification("Something went wrong", ScreenNotification.NotificationType.Red);
        }
    }

    private async void SearchEnterPressed(object sender, EventArgs e)
    {
        try
        {
            if (_searchBox is null)
            {
                return;
            }

            await ViewModel.Search();
        }
        catch (Exception reason)
        {
            logger.LogError(reason, "Failed to search for items");
            ScreenNotification.ShowNotification("Something went wrong", ScreenNotification.NotificationType.Red);
        }
    }
}