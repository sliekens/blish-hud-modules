using Blish_HUD;
using Blish_HUD.Graphics.UI;

using GuildWars2.Items;

using Microsoft.Extensions.Logging;

using SL.ChatLinks.UI.Tabs.Items.Services;
using SL.Common.Controls.Items;

namespace SL.ChatLinks.UI.Tabs.Items;

public class ItemsTabPresenter(
    IItemsTabView view,
    ItemsTabModel model,
    ILogger<ItemsTabPresenter> logger,
    ItemIcons icons,
    ItemSearch search
) : Presenter<IItemsTabView, ItemsTabModel>(view, model)
{
    public ItemIcons Icons { get; } = icons;

    private readonly CancellationTokenSource _loading = new(TimeSpan.FromMinutes(5));

    private readonly SemaphoreSlim _searchLock = new(1, 1);

    private EventHandler? _searchCancelled;

    public void CancelPendingSearches()
    {
        _searchCancelled?.Invoke(this, EventArgs.Empty);
    }

    public async Task Search(string text)
    {
        if (Program.IsMainThread)
        {
            await Task.Run(() => Search(text));
        }
        else
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
                    await DoSearch(text, cancellationTokenSource.Token);
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
    }

    public void ViewOptionSelected(Item item)
    {
        View.Select(item);
    }

    public async Task RefreshUpgrades()
    {
        Model.ClearUpgrades();
        await foreach (UpgradeComponent upgrade in search.OfType<UpgradeComponent>())
        {
            Model.AddUpgrade(upgrade);
        }
    }

    protected override async Task<bool> Load(IProgress<string> progress)
    {
        await foreach (UpgradeComponent upgrade in search.OfType<UpgradeComponent>())
        {
            Model.AddUpgrade(upgrade);
            progress.Report($"Loading upgrade components ({Model.Upgrades.Count})");
        }

        await foreach (Item item in search.NewItems(50).WithCancellation(_loading.Token))
        {
            Model.AddDefaultOption(item);
            progress.Report($"Loading newest items ({Model.DefaultOptions.Count()})");
        }

        return true;
    }

    protected override void UpdateView()
    {
        View.SetOptions(Model.DefaultOptions);
    }

    protected override void Unload()
    {
        _loading.Cancel();
        _loading.Dispose();
    }

    private async Task DoSearch(string text, CancellationToken cancellationToken)
    {
        string query = text.Trim();
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
        View.SetSearchLoading(true);
        try
        {
            View.ClearOptions();

            await foreach (Item item in search.Search(text, 100, cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                View.AddOption(item);
            }
        }
        finally
        {
            View.SetSearchLoading(false);

        }
    }

    private async Task NewItems(CancellationToken cancellationToken)
    {
        View.SetSearchLoading(true);
        try
        {
            Model.ClearDefaultOptions();
            View.ClearOptions();

            await foreach (Item item in search.NewItems(50).WithCancellation(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                Model.AddDefaultOption(item);
                View.AddOption(item);
            }
        }
        finally
        {
            View.SetSearchLoading(false);
        }
    }
}