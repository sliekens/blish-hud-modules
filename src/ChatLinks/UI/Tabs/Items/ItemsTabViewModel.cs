using System.Collections.ObjectModel;
using System.Windows.Input;

using Blish_HUD;

using GuildWars2.Items;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SL.ChatLinks.Storage;
using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.Common;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ItemsTabViewModel(
    ILogger<ItemsTabViewModel> logger,
    IOptionsMonitor<ChatLinkOptions> options,
    IEventAggregator eventAggregator,
    ItemSearch search,
    Customizer customizer,
    ItemsListViewModelFactory itemsListViewModelFactory,
    ChatLinkEditorViewModelFactory chatLinkEditorViewModelFactory)
    : ViewModel, IDisposable
{
    private string _searchText = "";

    private bool _searching;

    private int _resultTotal;

    private string _resultText = "";

    private EventHandler? _searchCancelled;

    private readonly SemaphoreSlim _searchLock = new(1, 1);

    public void Initialize()
    {
        eventAggregator.Subscribe<LocaleChanged>(OnLocaleChanged);
        eventAggregator.Subscribe<DatabaseSyncCompleted>(OnDatabaseSyncCompleted);
    }

    private async ValueTask OnLocaleChanged(LocaleChanged args)
    {
        await Task.Run(OnSearch);
    }

    private async ValueTask OnDatabaseSyncCompleted(DatabaseSyncCompleted args)
    {
        if (args.Updated["items"] > 0)
        {
            await Task.Run(OnSearch);
        }
    }

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

    public string ResultText
    {
        get => _resultText;
        set => SetField(ref _resultText, value);
    }

    public ObservableCollection<ItemsListViewModel> SearchResults { get; } = [];

    public int ResultTotal
    {
        get => _resultTotal;
        private set => SetField(ref _resultTotal, value);
    }

    public ICommand SearchCommand => new AsyncRelayCommand(async () =>
    {
        await Task.Run(OnSearch);
    });

    public async Task LoadAsync()
    {
        await customizer.LoadAsync();
        await NewItems(CancellationToken.None);
    }

    public ChatLinkEditorViewModel CreateChatLinkEditorViewModel(Item item)
    {
        return chatLinkEditorViewModelFactory.Create(item);
    }

    public void CancelPendingSearches()
    {
        _searchCancelled?.Invoke(this, EventArgs.Empty);
    }

    public async Task OnSearch()
    {
        CancelPendingSearches();
        using CancellationTokenSource cancellationTokenSource = new();
        _searchCancelled += SearchCancelled;
        try
        {
            // Debounce search
            await Task.Delay(300, cancellationTokenSource.Token);

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
            logger.LogDebug("Previous search was canceled");
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
        var main = Program.IsMainThread;
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

            int maxResults = options.CurrentValue.MaxResultCount;
            var context = new ResultContext();
            await foreach (Item item in search.Search(text, maxResults, context, cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var viewModel = itemsListViewModelFactory.Create(item, false);
                SearchResults.Add(viewModel);
            }

            ResultTotal = context.ResultTotal;
            ResultText = ResultTotal <= maxResults
                ? $"{ResultTotal:N0} matches"
                : $"{maxResults:N0} of {ResultTotal:N0} matches displayed";
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

            int maxResults = options.CurrentValue.MaxResultCount;
            await foreach (Item item in search.NewItems(maxResults).WithCancellation(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var viewModel = itemsListViewModelFactory.Create(item, false);
                SearchResults.Add(viewModel);
            }

            ResultTotal = await search.CountItems();
            ResultText = ResultTotal <= maxResults
                ? $"{ResultTotal:N0} items"
                : $"{maxResults:N0} of {ResultTotal:N0} items displayed";

        }
        finally
        {
            Searching = false;
        }
    }

    public void Dispose()
    {
        eventAggregator.Unsubscribe<DatabaseSyncCompleted>(OnDatabaseSyncCompleted);
    }
}