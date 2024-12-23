using System.Diagnostics.CodeAnalysis;

using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using CommunityToolkit.Diagnostics;

using GuildWars2.Items;

using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items.Controls;
using SL.ChatLinks.UI.Tabs.Items.Services;
using SL.Common.Controls.Items;

using Container = Blish_HUD.Controls.Container;
using Item = GuildWars2.Items.Item;

namespace SL.ChatLinks.UI.Tabs.Items;

public class ItemsTabView(ILogger<ItemsTabView> logger, ItemSearch search, ItemIcons icons) : View
{
    private readonly List<Item> _default = [];

    private readonly SemaphoreSlim _searchLock = new(1, 1);

    private readonly IDictionary<int, UpgradeComponent> _upgrades = new Dictionary<int, UpgradeComponent>();

    private Container? _root;

    private TextBox? _searchBox;

    private ItemsList? _searchResults;

    private ItemWidget? _selectedItem;

    private EventHandler? _searchUpdated;

    protected override async Task<bool> Load(IProgress<string> progress)
    {
        await foreach (UpgradeComponent upgrade in search.OfType<UpgradeComponent>())
        {
            _upgrades.Add(upgrade.Id, upgrade);
        }

        await foreach (Item item in search.NewItems(1000))
        {
            _default.Add(item);
        }

        return true;
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
        _searchResults = new ItemsList(icons, _upgrades)
        {
            Parent = buildPanel,
            Size = new Point(450, 500),
            Top = _searchBox.Bottom
        };

        _searchResults.SetOptions(_default);

        _searchBox.TextChanged += SearchTextChanged;
        _searchBox.EnterPressed += SearchInput;
        _searchBox.InputFocusChanged += (sender, args) =>
        {
            _searchBox.Text = "";
        };

        _searchResults.OptionClicked += ItemSelected;
    }

    protected override void Unload()
    {
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
                await DoSearch(_searchBox.Text);
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

            await DoSearch(_searchBox.Text);
        }
        catch (Exception reason)
        {
            logger.LogError(reason, "Failed to search for items");
            ScreenNotification.ShowNotification("Something went wrong", ScreenNotification.NotificationType.Red);
        }
    }

    private async Task DoSearch(string text)
    {
        _searchUpdated?.Invoke(this, EventArgs.Empty);

        bool searching = false;
        using CancellationTokenSource cancellationTokenSource = new();
        try
        {
            EnsureInitialized();

            searching = true;
            _searchUpdated += SearchUpdated;

            // Debounce search
            await Task.Delay(1000, cancellationTokenSource.Token);

            // Ensure exclusive access to the DbContext (not thread-safe)
            await _searchLock.WaitAsync(cancellationTokenSource.Token);
            try
            {
                _searchResults.SetLoading(true);
                string query = text.Trim();
                switch (query.Length)
                {
                    case 0:
                        _searchResults.SetOptions(_default);
                        _searchResults.SetLoading(false);
                        break;
                    case >= 3:
                        {
                            _searchResults.ClearOptions();
                            await foreach (Item item in search.Search(query, 100, cancellationTokenSource.Token))
                            {
                                _searchResults.AddOption(item);
                            }

                            searching = false;
                            _searchResults.SetLoading(false);
                            break;
                        }
                }
            }
            finally
            {
                _searchLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogDebug("Previous search was canceled");
        }
        finally
        {
            _searchUpdated -= SearchUpdated;
        }

        void SearchUpdated(object o, EventArgs a)
        {
            // ReSharper disable once AccessToModifiedClosure
            if (!searching)
            {
                return;
            }

            try
            {
                // ReSharper disable once AccessToDisposedClosure
                cancellationTokenSource.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Expected
            }
        }
    }

    private void ItemSelected(object sender, Item item)
    {
        EnsureInitialized();

        _selectedItem?.Dispose();
        _selectedItem = new ItemWidget(item, _upgrades, icons)
        {
            Parent = _root,
            Left = _searchResults.Right
        };
    }

    [MemberNotNull(
        nameof(_root),
        nameof(_searchBox),
        nameof(_searchResults))]
    private void EnsureInitialized()
    {
        if (_root is null)
        {
            ThrowHelper.ThrowInvalidOperationException("_root not initialized");
        }

        if (_searchBox is null)
        {
            ThrowHelper.ThrowInvalidOperationException("_searchBox not initialized");
        }

        if (_searchResults is null)
        {
            ThrowHelper.ThrowInvalidOperationException("_searchResults not initialized");
        }
    }
}