using System.Diagnostics.CodeAnalysis;

using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SL.ChatLinks.Storage;
using SL.Common;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items2;

public sealed class ItemsTabViewModel : ViewModel
{
    private string _searchText = "";

    private IReadOnlyDictionary<int, UpgradeComponent>? _upgrades;

    private readonly ILogger<ItemsTabViewModel> _logger;

    private readonly ChatLinksContext _context;

    public ItemsTabViewModel(ILogger<ItemsTabViewModel> logger,
        ChatLinksContext context)
    {
        _logger = logger;
        _context = context;
        SearchCommand = new RelayCommand(Search);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetField(ref _searchText, value);
    }

    public RelayCommand SearchCommand { get; }

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

    public async void Search()
    {
        _logger.LogInformation("TODO: search {Text}", SearchText);
    }

    public void Unload()
    {
        MessageBus.Unregister("items_tab");
    }
}