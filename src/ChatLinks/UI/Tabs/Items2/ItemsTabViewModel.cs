using System.Diagnostics.CodeAnalysis;

using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SL.ChatLinks.Storage;
using SL.ChatLinks.UI.Tabs.Items.Services;
using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items2;

public sealed class ItemsTabViewModel(
    ILogger<ItemsTabViewModel> logger,
    ChatLinksContext context,
    ItemSearch search
) : ViewModel
{
    private string _searchText = "";

    private IReadOnlyDictionary<int, UpgradeComponent>? _upgrades;

    public string SearchText
    {
        get => _searchText;
        set => SetField(ref _searchText, value);
    }

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
        Upgrades = await context.Set<UpgradeComponent>().ToDictionaryAsync(upgrade => upgrade.Id);

        MessageBus.Register("items_tab", async void (message) =>
        {
            try
            {
                if (message == "refresh")
                {
                    Upgrades = await context.Set<UpgradeComponent>().ToDictionaryAsync(upgrade => upgrade.Id);
                }
            }
            catch (Exception reason)
            {
                logger.LogError(reason, "Failed to process message: {Message}", message);
            }
        });
    }

    public async Task Search()
    {
        logger.LogInformation("TODO: search {Text}", SearchText);
    }

    public void Unload()
    {
        MessageBus.Unregister("items_tab");
    }
}