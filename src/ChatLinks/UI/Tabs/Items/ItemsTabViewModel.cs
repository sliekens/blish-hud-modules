﻿using System.Collections.ObjectModel;
using System.Windows.Input;

using GuildWars2.Items;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SL.ChatLinks.Storage;
using SL.ChatLinks.UI.Tabs.Items.Collections;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ItemsTabViewModel(
    ILogger<ItemsTabViewModel> logger,
    IStringLocalizer<ItemsTabView> localizer,
    IOptionsMonitor<ChatLinkOptions> options,
    IEventAggregator eventAggregator,
    ItemSearch search,
    ItemsListViewModel.Factory itemsListViewModelFactory,
    ChatLinkEditorViewModel.Factory chatLinkEditorViewModelFactory)
    : ViewModel, IDisposable
{
    public delegate ItemsTabViewModel Factory();

    private string _searchText = "";

    private bool _searching;

    private int _resultTotal;

    private string _resultText = "";

    private EventHandler? _searchCancelled;

    private Item? _selectedItem;

    private readonly SemaphoreSlim _searchLock = new(1, 1);

    public Item? SelectedItem
    {
        get => _selectedItem;
        set => SetField(ref _selectedItem, value);
    }

    public void Initialize()
    {
        eventAggregator.Subscribe<LocaleChanged>(OnLocaleChanged);
        eventAggregator.Subscribe<DatabaseDownloaded>(OnDatabaseDownloaded);
        eventAggregator.Subscribe<DatabaseSeeded>(OnDatabaseSeeded);
    }

    private async Task OnLocaleChanged(LocaleChanged args)
    {
        OnPropertyChanged(nameof(SearchPlaceholderText));
        await Task.Run(OnSearch).ConfigureAwait(false);
    }

    private async Task OnDatabaseDownloaded(DatabaseDownloaded downloaded)
    {
        await Task.Run(OnSearch).ConfigureAwait(false);
    }

    private async Task OnDatabaseSeeded(DatabaseSeeded args)
    {
        if (args.Updated["items"] > 0)
        {
            await Task.Run(OnSearch).ConfigureAwait(false);
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
        await Task.Run(OnSearch).ConfigureAwait(false);
    });

    public string SearchPlaceholderText => localizer["Search placeholder"];

    public async Task LoadAsync()
    {
        await Task.Run(async () => await NewItems(CancellationToken.None).ConfigureAwait(false)).ConfigureAwait(false);
    }

    public ChatLinkEditorViewModel CreateChatLinkEditorViewModel(Item item)
    {
        return chatLinkEditorViewModelFactory(item);
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
            await Task.Delay(300, cancellationTokenSource.Token).ConfigureAwait(false);

            // Ensure exclusive access to the DbContext (not thread-safe)
            await _searchLock.WaitAsync(cancellationTokenSource.Token).ConfigureAwait(false);
            try
            {
                await DoSearch(cancellationTokenSource.Token).ConfigureAwait(false);
            }
            finally
            {
                _ = _searchLock.Release();
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
        string query = SearchText.Trim();
        switch (query.Length)
        {
            case 0:
                await NewItems(cancellationToken).ConfigureAwait(false);
                break;
            case >= 3:
                await Query(query, cancellationToken).ConfigureAwait(false);
                break;
            default:
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
            ResultContext context = new();
            await foreach (Item item in search.Search(text, maxResults, context, cancellationToken).ConfigureAwait(false))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                ItemsListViewModel viewModel = itemsListViewModelFactory(item, item.Id == SelectedItem?.Id);
                SearchResults.Add(viewModel);
            }

            ResultTotal = context.ResultTotal;
            ResultText = ResultTotal <= maxResults
                ? localizer["Total results", ResultTotal]
                : localizer["Partial results", maxResults, ResultTotal];
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

                ItemsListViewModel viewModel = itemsListViewModelFactory(item, item.Id == SelectedItem?.Id);
                SearchResults.Add(viewModel);
            }

            ResultTotal = await search.CountItems().ConfigureAwait(false);
            ResultText = ResultTotal <= maxResults
                ? localizer["Total results", ResultTotal]
                : localizer["Partial results", maxResults, ResultTotal];

        }
        finally
        {
            Searching = false;
        }
    }

    public void Dispose()
    {
        eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
        eventAggregator.Unsubscribe<DatabaseDownloaded>(OnDatabaseDownloaded);
        eventAggregator.Unsubscribe<DatabaseSeeded>(OnDatabaseSeeded);
        _searchLock.Dispose();
        GC.SuppressFinalize(this);
    }
}
