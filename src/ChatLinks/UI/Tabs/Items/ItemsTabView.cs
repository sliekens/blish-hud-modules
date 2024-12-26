using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items.Controls;
using SL.ChatLinks.UI.Tabs.Items.Services;

using Container = Blish_HUD.Controls.Container;
using Item = GuildWars2.Items.Item;

namespace SL.ChatLinks.UI.Tabs.Items;

public class ItemsTabView(ILogger<ItemsTabView> logger) : View<ItemsTabPresenter>, IItemsTabView
{
    private Container? _root;

    private TextBox? _searchBox;

    private ItemsList? _searchResults;

    private ItemWidget? _selectedItem;

    public void SetSearchLoading(bool loading)
    {
        _searchResults?.SetLoading(loading);
    }

    public void AddOption(Item item)
    {
        _searchResults?.AddOption(item);
    }

    public void SetOptions(IEnumerable<Item> items)
    {
        _searchResults?.SetOptions(items);
    }

    public void ClearOptions()
    {
        _searchResults?.ClearOptions();
    }

    public void Select(Item item)
    {
        _selectedItem?.Dispose();
        _selectedItem = new ItemWidget(item, Presenter.Icons, Presenter.Model.Upgrades)
        {
            Parent = _root,
            Left = _searchResults!.Right
        };
    }

    protected override void Build(Container buildPanel)
    {
        _root = buildPanel;
        _searchBox = new TextBox
        {
            Parent = buildPanel,
            Width = 450,
            PlaceholderText = "Enter item name or chat link..."
        };
        _searchResults = new ItemsList(Presenter.Icons, Presenter.Model.Upgrades)
        {
            Parent = buildPanel,
            Size = new Point(450, 500),
            Top = _searchBox.Bottom
        };

        _searchBox.TextChanged += SearchTextChanged;
        _searchBox.EnterPressed += SearchInput;
        _searchBox.InputFocusChanged += (sender, args) =>
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

        _searchResults.OptionClicked += (sender, item) =>
        {
            Presenter.ViewOptionSelected(item);
        };
    }

    protected override void OnPresenterAssigned(ItemsTabPresenter presenter)
    {
        MessageBus.Register("items_tab", async void (message) =>
        {
            try
            {
                if (message == "refresh")
                {
                    await presenter.RefreshUpgrades();
                }
            }
            catch (Exception reason)
            {
                logger.LogError(reason, "Failed to process message: {Message}", message);
            }
        });
    }

    protected override void Unload()
    {
        MessageBus.Unregister("items_tab");
        if (_searchBox is not null)
        {
            _searchBox.TextChanged -= SearchInput;
        }

        _searchBox?.Dispose();
        _searchResults?.Dispose();
        base.Unload();
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
                await Presenter.Search(_searchBox.Text);
            }
        }
        catch (Exception reason)
        {
            logger.LogError(reason, "Failed to search for items");
            ScreenNotification.ShowNotification("Something went wrong", ScreenNotification.NotificationType.Red);
        }
    }

    private async void SearchInput(object sender, EventArgs e)
    {
        try
        {
            if (_searchBox is null)
            {
                return;
            }

            await Presenter.Search(_searchBox.Text);
        }
        catch (Exception reason)
        {
            logger.LogError(reason, "Failed to search for items");
            ScreenNotification.ShowNotification("Something went wrong", ScreenNotification.NotificationType.Red);
        }
    }
}