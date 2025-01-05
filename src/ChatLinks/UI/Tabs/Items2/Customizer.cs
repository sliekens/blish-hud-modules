using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SL.ChatLinks.Storage;

namespace SL.ChatLinks.UI.Tabs.Items2;

public sealed class Customizer(
    ILogger<Customizer> logger,
    ChatLinksContext context
) : IDisposable, IAsyncDisposable
{
    public IReadOnlyDictionary<int, UpgradeComponent> UpgradeComponents { get; private set; } =
        new Dictionary<int, UpgradeComponent>(0);

    public async Task LoadAsync()
    {
        UpgradeComponents =
            await context.Set<UpgradeComponent>().AsNoTracking().ToDictionaryAsync(upgrade => upgrade.Id);
        MessageBus.Register("customizer", async void (message) =>
        {
            try
            {
                if (message == "refresh")
                {
                    UpgradeComponents = await context.Set<UpgradeComponent>().AsNoTracking()
                        .ToDictionaryAsync(upgrade => upgrade.Id);
                }
            }
            catch (Exception reason)
            {
                logger.LogError(reason, "Failed to process message: {Message}", message);
            }
        });
    }

    public void Dispose()
    {
        MessageBus.Unregister("customizer");
        context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        MessageBus.Unregister("customizer");
        await context.DisposeAsync();
    }
}