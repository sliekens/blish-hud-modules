using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using ChatLinksModule.Storage;
using ChatLinksModule.UI.Tabs.Items.Controls;

using CommunityToolkit.Diagnostics;

using GuildWars2.Chat;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

using Container = Blish_HUD.Controls.Container;
using Item = GuildWars2.Items.Item;

namespace ChatLinksModule.UI.Tabs.Items;

public class ItemsTabView(ChatLinksContext db, ILogger<ItemsTabView> logger) : View
{
    private static readonly Regex Pattern = new(@"^\[&[A-Za-z0-9+/=]+\]$", RegexOptions.Compiled);

    private readonly SemaphoreSlim _searchLock = new(1, 1);

    private Container? _root;

    private TextBox? _searchBox;

    private ItemsList? _searchResults;

    private ItemWidget? _selectedItem;

    protected override void Build(Container buildPanel)
    {
        _root = buildPanel;
        _searchBox = new TextBox
        {
            Parent = buildPanel, Width = 450, PlaceholderText = "Enter item name or chat link..."
        };
        _searchResults = new ItemsList { Parent = buildPanel, Size = new Point(450, 500), Top = _searchBox.Bottom };

        _searchResults.SetOptions(db.Items.OrderByDescending(item => item.Id).Take(100).AsEnumerable());

        _searchBox.TextChanged += SearchInput;
        _searchResults.OptionClicked += ItemSelected;
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

                string search = _searchBox.Text.Trim();
                if (search.Length == 0)
                {
                    results = await db.Items.OrderByDescending(item => item.Id).Take(100).ToListAsync();
                }
                else if (search.Length >= 3)
                {
                    // Debounce search
                    await Task.Delay(300, cancellationTokenSource.Token);

                    if (Pattern.IsMatch(search))
                    {
                        ItemLink link = ItemLink.Parse(search);
                        if (await db.Items.FindAsync(link.ItemId) is { } item)
                        {
                            results.Add(item);

                            if (item is Weapon weapon)
                            {
                                if (weapon.SuffixItemId.HasValue &&
                                    await db.Items.FindAsync(weapon.SuffixItemId.Value) is { } suffixItem)
                                {
                                    results.Add(suffixItem);
                                }

                                if (weapon.SecondarySuffixItemId.HasValue &&
                                    await db.Items.FindAsync(weapon.SecondarySuffixItemId.Value) is
                                        { } secondarySuffixItem)
                                {
                                    results.Add(secondarySuffixItem);
                                }
                            }

                            if (item is Armor armor)
                            {
                                if (armor.SuffixItemId.HasValue && await db.Items.FindAsync(armor.SuffixItemId.Value) is
                                        { } suffixItem)
                                {
                                    results.Add(suffixItem);
                                }
                            }

                            if (item is Backpack back)
                            {
                                if (back.SuffixItemId.HasValue && await db.Items.FindAsync(back.SuffixItemId.Value) is
                                        { } suffixItem)
                                {
                                    results.Add(suffixItem);
                                }
                            }

                            if (item is Trinket trinket)
                            {
                                if (trinket.SuffixItemId.HasValue &&
                                    await db.Items.FindAsync(trinket.SuffixItemId.Value) is { } suffixItem)
                                {
                                    results.Add(suffixItem);
                                }
                            }
                        }
                    }
                    else
                    {
                        results = await db.Items
                            .Where(i => i.Name.ToLower().Contains(search.ToLowerInvariant()))
                            .Take(100)
                            .ToListAsync(cancellationTokenSource.Token);
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
            Parent = _root, Left = _searchResults.Right
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