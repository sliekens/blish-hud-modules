using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using CommunityToolkit.Diagnostics;

using GuildWars2.Chat;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

using SL.ChatLinks.Storage;
using SL.ChatLinks.UI.Tabs.Items.Controls;
using SL.ChatLinks.UI.Tabs.Items.Services;

using Container = Blish_HUD.Controls.Container;
using Item = GuildWars2.Items.Item;

namespace SL.ChatLinks.UI.Tabs.Items;

public class ItemsTabView(ILogger<ItemsTabView> logger, ItemSearch search) : View
{
    private readonly SemaphoreSlim _searchLock = new(1, 1);

    private Container? _root;

    private TextBox? _searchBox;

    private ItemsList? _searchResults;

    private ItemWidget? _selectedItem;

    private readonly List<Item> _default = [];

    protected override void Build(Container buildPanel)
    {
        _root = buildPanel;
        _searchBox = new TextBox
        {
            Parent = buildPanel,
            Width = 450,
            PlaceholderText = "Enter item name or chat link..."
        };
        _searchResults = new ItemsList { Parent = buildPanel, Size = new Point(450, 500), Top = _searchBox.Bottom };

        _searchResults.SetOptions(_default);

        _searchBox.TextChanged += SearchInput;
        _searchResults.OptionClicked += ItemSelected;
    }

    protected override async Task<bool> Load(IProgress<string> progress)
    {
        await foreach (var item in search.NewItems(100))
        {
            _default.Add(item);
        }

        return true;
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

    private async void SearchInput(object sender, EventArgs e)
    {
        try
        {
            EnsureInitialized();

            using CancellationTokenSource cancellationTokenSource = new();
            bool searching = true;
            _searchResults.SetLoading(true);
            _searchBox.TextChanged += OnTextChangedAgain;

            List<Item> results = [];
            try
            {
                // Ensure exclusive access to the DbContext (not thread-safe)
                await _searchLock.WaitAsync(cancellationTokenSource.Token);

                string query = _searchBox.Text.Trim();
                if (query.Length == 0)
                {
                    results = _default;
                }
                else if (query.Length >= 3)
                {
                    // Debounce search
                    await Task.Delay(300, cancellationTokenSource.Token);

                    await foreach (var item in search.Search(query, cancellationTokenSource.Token))
                    {
                        results.Add(item);
                    }
                }

                searching = false;
                _searchResults.SetOptions(results);
                _searchResults.SetLoading(false);
            }
            finally
            {
                _searchBox.TextChanged -= OnTextChangedAgain;
                _searchLock.Release();
            }

            void OnTextChangedAgain(object o, EventArgs e)
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
        catch (OperationCanceledException)
        {
            logger.LogDebug("Previous search was canceled");
        }
        catch (Exception reason)
        {
            logger.LogError(reason, "Failed to search for items");
            ScreenNotification.ShowNotification("Something went wrong", ScreenNotification.NotificationType.Red);
        }
    }

    private void ItemSelected(object sender, Item item)
    {
        EnsureInitialized();

        _selectedItem?.Dispose();
        _selectedItem = new ItemWidget(item)
        {
            Parent = _root,
            Left = _searchResults.Right
        };
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
}
