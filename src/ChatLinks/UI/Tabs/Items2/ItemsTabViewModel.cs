using System.Collections.ObjectModel;
using System.Windows.Input;

using GuildWars2.Items;

using Microsoft.Extensions.Logging;

using SL.ChatLinks.UI.Tabs.Items.Services;
using SL.ChatLinks.UI.Tabs.Items2.Content;
using SL.ChatLinks.UI.Tabs.Items2.Search;
using SL.Common;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items2;

public sealed class ItemsTabViewModel : ViewModel
{
    private readonly ILogger<ItemsTabViewModel> _logger;

    private readonly ItemSearch _search;

    private readonly Customizer _customizer;

    private readonly ItemsListViewModelFactory _itemsListViewModelFactory;

    private readonly ChatLinkEditorViewModelFactory _chatLinkEditorViewModelFactory;

    private string _searchText = "";

    private bool _searching;

    private EventHandler? _searchCancelled;

    private readonly SemaphoreSlim _searchLock = new(1, 1);

    public ItemsTabViewModel(
        ILogger<ItemsTabViewModel> logger,
        ItemSearch search,
        Customizer customizer,
        ItemsListViewModelFactory itemsListViewModelFactory,
        ChatLinkEditorViewModelFactory chatLinkEditorViewModelFactory
    )
    {
        _logger = logger;
        _search = search;
        _customizer = customizer;
        _itemsListViewModelFactory = itemsListViewModelFactory;
        _chatLinkEditorViewModelFactory = chatLinkEditorViewModelFactory;
        SearchCommand = new AsyncRelayCommand(Search);
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

    public ObservableCollection<ItemsListViewModel> SearchResults { get; } = [];

    public ICommand SearchCommand { get; }

    public async Task LoadAsync()
    {
        await _customizer.LoadAsync();
        await NewItems(CancellationToken.None);
    }

    public ChatLinkEditorViewModel CreateChatLinkEditorViewModel(Item item)
    {
        return _chatLinkEditorViewModelFactory.Create(item);
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

                var viewModel = _itemsListViewModelFactory.Create(item);
                SearchResults.Add(viewModel);
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

                var viewModel = _itemsListViewModelFactory.Create(item);
                SearchResults.Add(viewModel);
            }
        }
        finally
        {
            Searching = false;
        }
    }
}