using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;

using SL.ChatLinks.Storage;
using SL.Common;
using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items2;

public sealed class Customizer(
    ChatLinksContext context,
    IEventAggregator eventAggregator
) : IDisposable, IAsyncDisposable
{
    public IReadOnlyDictionary<int, UpgradeComponent> UpgradeComponents { get; private set; } =
        new Dictionary<int, UpgradeComponent>(0);

    public async Task LoadAsync()
    {
        UpgradeComponents =
            await context.Set<UpgradeComponent>().AsNoTracking().ToDictionaryAsync(upgrade => upgrade.Id);

        eventAggregator.Subscribe<DatabaseSyncCompleted>(OnDatabaseSyncCompleted);
    }

    private async ValueTask OnDatabaseSyncCompleted(DatabaseSyncCompleted _)
    {
        UpgradeComponents = await context.Set<UpgradeComponent>().AsNoTracking()
            .ToDictionaryAsync(upgrade => upgrade.Id);
    }

    public void Dispose()
    {
        eventAggregator.Unsubscribe<DatabaseSyncCompleted>(OnDatabaseSyncCompleted);
        context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        eventAggregator.Unsubscribe<DatabaseSyncCompleted>(OnDatabaseSyncCompleted);
        await context.DisposeAsync();
    }

    public IEnumerable<UpgradeComponent> GetUpgradeComponents(Item targetItem, UpgradeSlotType slotType)
    {
        return UpgradeComponents.Values.Where(component => FilterUpgradeSlot(targetItem, slotType, component));
    }

    public UpgradeComponent? DefaultSuffixItem(Item item)
    {
        if (item is not IUpgradable upgradable)
        {
            return null;
        }

        return GetUpgradeComponent(upgradable.SuffixItemId)
            ?? GetUpgradeComponent(upgradable.SecondarySuffixItemId);
    }

    public UpgradeComponent? GetUpgradeComponent(int? upgradeComponentId)
    {
        if (upgradeComponentId.HasValue && UpgradeComponents.TryGetValue(upgradeComponentId.Value, out var upgradeComponent))
        {
            return upgradeComponent;
        }

        return null;
    }

    private bool FilterUpgradeSlot(Item targetItem, UpgradeSlotType slotType, UpgradeComponent component)
    {
        if (slotType == UpgradeSlotType.Infusion)
        {
            return component.InfusionUpgradeFlags.Infusion;
        }

        if (component.InfusionUpgradeFlags.Infusion)
        {
            return false;
        }

        if (slotType == UpgradeSlotType.Enrichment)
        {
            return component.InfusionUpgradeFlags.Enrichment;
        }

        if (component.InfusionUpgradeFlags.Enrichment)
        {
            return false;
        }

        if (component is Gem)
        {
            return true;
        }

        return targetItem switch
        {
            Armor armor when armor.WeightClass == WeightClass.Light => component.UpgradeComponentFlags.LightArmor,
            Armor armor when armor.WeightClass == WeightClass.Medium => component.UpgradeComponentFlags.MediumArmor,
            Armor armor when armor.WeightClass == WeightClass.Heavy => component.UpgradeComponentFlags.HeavyArmor,
            Axe => component.UpgradeComponentFlags.Axe,
            Dagger => component.UpgradeComponentFlags.Dagger,
            Focus => component.UpgradeComponentFlags.Focus,
            Greatsword => component.UpgradeComponentFlags.Greatsword,
            Hammer => component.UpgradeComponentFlags.Hammer,
            HarpoonGun => component.UpgradeComponentFlags.HarpoonGun,
            Longbow => component.UpgradeComponentFlags.LongBow,
            Mace => component.UpgradeComponentFlags.Mace,
            Pistol => component.UpgradeComponentFlags.Pistol,
            Rifle => component.UpgradeComponentFlags.Rifle,
            Scepter => component.UpgradeComponentFlags.Scepter,
            Shield => component.UpgradeComponentFlags.Shield,
            Shortbow => component.UpgradeComponentFlags.ShortBow,
            Spear => component.UpgradeComponentFlags.Spear,
            Staff => component.UpgradeComponentFlags.Staff,
            Sword => component.UpgradeComponentFlags.Sword,
            Torch => component.UpgradeComponentFlags.Torch,
            Trident => component.UpgradeComponentFlags.Trident,
            Trinket => component.UpgradeComponentFlags.Trinket,
            Warhorn => component.UpgradeComponentFlags.Warhorn,
            _ => true
        };
    }
}