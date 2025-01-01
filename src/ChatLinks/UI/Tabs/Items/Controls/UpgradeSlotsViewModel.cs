using GuildWars2.Hero;
using GuildWars2.Items;

using SL.Common;
using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public sealed class UpgradeSlotsViewModel : ViewModel
{
    public event Action? UpgradesChanged;

    public Item? Item { get; set; }

    public IReadOnlyDictionary<int, UpgradeComponent>? UpgradeComponents { get; set; }

    public IReadOnlyList<UpgradeSlotModel>? UpgradeSlots { get; private set; }

    public IReadOnlyList<UpgradeComponent>? UpgradeOptions { get; private set; }

    public IReadOnlyList<UpgradeSlotModel>? InfusionSlots { get; private set; }

    public IReadOnlyList<UpgradeComponent>? InfusionOptions { get; private set; }

    public IReadOnlyList<UpgradeComponent>? EnrichmentOptions { get; private set; }

    public int? SuffixItemId => UpgradeSlots?.FirstOrDefault()?.SelectedUpgradeComponent?.Id;

    public int? SecondarySuffixItemId => UpgradeSlots?.Skip(1).FirstOrDefault()?.SelectedUpgradeComponent?.Id;

    public void Initialize()
    {
        if (Item is null || UpgradeComponents is null)
        {
            throw new InvalidOperationException();
        }

        UpgradeSlots = SetupUpgradeSlots(Item);
        InfusionSlots = SetupInfusionSlots(Item);
        UpgradeOptions = UpgradeComponents.Values
            .Where(component => FilterUpgradeSlot(UpgradeSlotType.Default, component))
            .ToList()
            .AsReadOnly();
        InfusionOptions = UpgradeComponents.Values
            .Where(component => FilterUpgradeSlot(UpgradeSlotType.Infusion, component))
            .ToList()
            .AsReadOnly();
        EnrichmentOptions = UpgradeComponents.Values
            .Where(component => FilterUpgradeSlot(UpgradeSlotType.Enrichment, component))
            .ToList()
            .AsReadOnly();
    }

    public IEnumerable<UpgradeSlotModel> Slots()
    {
        if (UpgradeSlots is null || InfusionSlots is null)
        {
            throw new InvalidOperationException();
        }

        return UpgradeSlots.Concat(InfusionSlots);
    }

    public UpgradeComponent? EffectiveSuffixItem()
    {
        if (UpgradeSlots is null)
        {
            throw new InvalidOperationException();
        }

        return UpgradeSlots.Select(pair => pair.EffectiveUpgrade)
            .FirstOrDefault(upgrade => upgrade is not null);
    }

    public UpgradeSlotModel? UpgradeSlot1 => UpgradeSlots?.FirstOrDefault();

    public UpgradeSlotModel? UpgradeSlot2 => UpgradeSlots?.Skip(1).FirstOrDefault();

    public IReadOnlyList<InfusionSlot> Infusions()
    {
        if (InfusionSlots is null)
        {
            throw new InvalidOperationException();
        }

        return InfusionSlots
            .Select(slot => new InfusionSlot
            {
                ItemId = slot.EffectiveUpgrade?.Id,
                Flags = new InfusionSlotFlags
                {
                    Infusion = slot.Type == UpgradeSlotType.Infusion,
                    Enrichment = slot.Type == UpgradeSlotType.Enrichment,
                    Other = []
                }
            })
            .ToList()
            .AsReadOnly();
    }

    private IReadOnlyList<UpgradeSlotModel> SetupUpgradeSlots(Item item)
    {
        if (UpgradeComponents is null)
        {
            throw new InvalidOperationException();
        }

        if (item is not IUpgradable upgradable)
        {
            return [];
        }

        return upgradable.UpgradeSlots
            .Select(upgradeComponentId => new UpgradeSlotModel
            {
                Type = UpgradeSlotType.Default,
                DefaultUpgradeComponent = DefaultUpgradeComponent(upgradeComponentId)
            })
            .ToList()
            .AsReadOnly();
    }

    private IReadOnlyList<UpgradeSlotModel> SetupInfusionSlots(Item item)
    {
        if (UpgradeComponents is null)
        {
            throw new InvalidOperationException();
        }

        if (item is not IUpgradable upgradable)
        {
            return [];
        }

        return upgradable.InfusionSlots
            .Select(infusionSlot => new UpgradeSlotModel
            {
                Type = infusionSlot.Flags switch
                {
                    { Infusion: true } => UpgradeSlotType.Infusion,
                    { Enrichment: true } => UpgradeSlotType.Enrichment,
                    _ => UpgradeSlotType.Default
                },
                DefaultUpgradeComponent = DefaultUpgradeComponent(infusionSlot.ItemId)
            })
            .ToList()
            .AsReadOnly();
    }

    public UpgradeComponent? DefaultUpgradeComponent(int? upgradeComponentId)
    {
        if (UpgradeComponents is null)
        {
            throw new InvalidOperationException();
        }

        if (upgradeComponentId.HasValue
            && UpgradeComponents.TryGetValue(upgradeComponentId.Value, out var upgradeComponent)
        )
        {
            return upgradeComponent;

        }

        return null;
    }

    private bool FilterUpgradeSlot(UpgradeSlotType slotType, UpgradeComponent component)
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

        return Item switch
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

    public void OnUpgradeChanged()
    {
        UpgradesChanged?.Invoke();
    }
}