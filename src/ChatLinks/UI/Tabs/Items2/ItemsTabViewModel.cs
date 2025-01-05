using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SL.ChatLinks.Storage;
using SL.ChatLinks.UI.Tabs.Items.Services;
using SL.Common;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items2;

public sealed class ItemsTabViewModel : ViewModel
{
    private readonly ILogger<ItemsTabViewModel> _logger;

    private readonly ChatLinksContext _context;

    private readonly ItemSearch _search;

    private string _searchText = "";

    private bool _searching;

    private EventHandler? _searchCancelled;

    private readonly SemaphoreSlim _searchLock = new(1, 1);

    private IReadOnlyDictionary<int, UpgradeComponent>? _upgrades;

    public ItemsTabViewModel(
        ILogger<ItemsTabViewModel> logger,
        ChatLinksContext context,
        ItemSearch search,
        ItemsListViewModel itemsListViewModel
    )
    {
        _logger = logger;
        _context = context;
        _search = search;
        ItemsListViewModel = itemsListViewModel;
        SearchCommand = new AsyncRelayCommand(Search);
    }
    public ItemsListViewModel ItemsListViewModel { get; }

    public string SearchText
    {
        get => _searchText;
        set => SetField(ref _searchText, value);
    }

    public bool Searching
    {
        get => _searching;
        set => SetField(ref _searching, value);
    }

    public ObservableCollection<Item> SearchResults { get; } = [];

    public AsyncRelayCommand SearchCommand { get; }

    public IReadOnlyDictionary<int, UpgradeComponent>? Upgrades
    {
        get => _upgrades;
        private set => SetField(ref _upgrades, value);
    }

    [MemberNotNull(nameof(Upgrades))]
    public void EnsureLoaded()
    {
        if (Upgrades is null)
        {
            throw new InvalidOperationException();
        }
    }

    public async Task LoadAsync()
    {
        Upgrades = await _context.Set<UpgradeComponent>().ToDictionaryAsync(upgrade => upgrade.Id);
        await NewItems(CancellationToken.None);

        MessageBus.Register("items_tab", async void (message) =>
        {
            try
            {
                if (message == "refresh")
                {
                    Upgrades = await _context.Set<UpgradeComponent>().ToDictionaryAsync(upgrade => upgrade.Id);
                }
            }
            catch (Exception reason)
            {
                _logger.LogError(reason, "Failed to process message: {Message}", message);
            }
        });
    }

    public void CancelPendingSearches()
    {
        _searchCancelled?.Invoke(this, EventArgs.Empty);
    }

    public async Task Search()
    {
        CancelPendingSearches();
        using CancellationTokenSource cancellationTokenSource = new();
        _searchCancelled += SearchCancelled;
        try
        {
            // Debounce search
            await Task.Delay(1000, cancellationTokenSource.Token);

            // Ensure exclusive access to the DbContext (not thread-safe)
            await _searchLock.WaitAsync(cancellationTokenSource.Token);
            try
            {
                await DoSearch(cancellationTokenSource.Token);
            }
            finally
            {
                _searchLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Previous search was canceled");
        }
        finally
        {
            _searchCancelled -= SearchCancelled;
        }

        void SearchCancelled(object o, EventArgs a)
        {
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

    private async Task DoSearch(CancellationToken cancellationToken)
    {
        string query = SearchText.Trim();
        switch (query.Length)
        {
            case 0:
                await NewItems(cancellationToken);
                break;
            case >= 3:
                await Query(query, cancellationToken);
                break;
        }
    }
    private async Task Query(string text, CancellationToken cancellationToken)
    {
        Searching = true;
        try
        {
            SearchResults.Clear();

            await foreach (Item item in _search.Search(text, 100, cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                SearchResults.Add(item);
            }
        }
        finally
        {
            Searching = false;
        }
    }

    private async Task NewItems(CancellationToken cancellationToken)
    {
        Searching = true;
        try
        {
            SearchResults.Clear();

            await foreach (Item item in _search.NewItems(50).WithCancellation(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                SearchResults.Add(item);
            }
        }
        finally
        {
            Searching = false;
        }
    }


    public void Unload()
    {
        MessageBus.Unregister("items_tab");
    }
}