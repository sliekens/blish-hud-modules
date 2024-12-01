using System.Diagnostics.CodeAnalysis;

using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;

using ChatLinksModule.Storage;
using ChatLinksModule.UI.Tabs.Items.Controls;

using CommunityToolkit.Diagnostics;

using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

using Container = Blish_HUD.Controls.Container;

namespace ChatLinksModule.UI.Tabs.Items;

public class ItemsTabView(ChatLinksContext db, ILogger<ItemsTabView> logger) : View
{
    private readonly SemaphoreSlim _searchLock = new(1, 1);

    private ItemsList? _searchResults;

    private Container? _root;

    private TextBox? _searchBox;

    private ItemWidget? _selectedItem;

    protected override void Build(Container buildPanel)
    {
        _root = buildPanel;
        _searchBox = new TextBox { Parent = buildPanel, Width = 450 };
        _searchResults = new ItemsList
        {
            Parent = buildPanel,
            Size = new Point(450, 500),
            Top = _searchBox.Bottom
        };

        _searchBox.TextChanged += SearchInput;
        _searchResults.Click += ItemClicked;
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

            using var cancellationTokenSource = new CancellationTokenSource();
            var searching = true;
            _searchResults.SetLoading(true);
            _searchBox.TextChanged += OnTextChangedAgain;

            List<Item> results = [];
            try
            {
                // Ensure exclusive access to the DbContext (not thread-safe)
                await _searchLock.WaitAsync(cancellationTokenSource.Token);

                string search = _searchBox.Text.ToLowerInvariant().Trim();
                if (search.Length >= 3)
                {
                    // Debounce search
                    await Task.Delay(300, cancellationTokenSource.Token);

                    results = await db.Items
                        .Where(i => i.Name.ToLower().Contains(search))
                        .Take(100)
                        .ToListAsync(cancellationToken: cancellationTokenSource.Token);
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

    private void ItemClicked(object sender, MouseEventArgs e)
    {
        EnsureInitialized();

        ItemChoice? clickedItem = _searchResults.Children.OfType<ItemChoice>()
            .SingleOrDefault(entry => entry.AbsoluteBounds.Contains(e.MousePosition));
        ShowWidget(clickedItem?.Item);
    }

    private void ShowWidget(Item? item)
    {
        EnsureInitialized();

        _selectedItem?.Dispose();
        _selectedItem = item is not null ? new ItemWidget(item)
        {
            Parent = _root,
            Left = _searchResults.Right,
            Width = _root.Right - _searchResults.Right
        } : null;
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