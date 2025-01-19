using Blish_HUD.Content;

using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items.Tooltips;

public sealed class ItemTooltipViewModel(
    ILogger<ItemTooltipViewModel> logger,
    ItemIcons icons,
    Customizer customizer,
    Hero hero,
    Item item,
    int quantity,
    IEnumerable<UpgradeSlot> upgrades
) : ViewModel
{
    private bool? _unlocked;

    private bool _locked;

    public IReadOnlyList<UpgradeSlot> UpgradesSlots { get; } = upgrades.ToList();

    public Item Item { get; } = item;

    public bool UnlocksAvailable => hero.UnlocksAvailable;

    public bool DefaultLocked
    {
        get => _locked;
        set => SetField(ref _locked, value);
    }

    public bool? Unlocked
    {
        get => _unlocked;
        set => SetField(ref _unlocked, value);
    }

    public int Quantity { get; } = quantity;

    public string? DefaultSuffixName { get; } = customizer.DefaultSuffixItem(item)?.SuffixName;

    public Color ItemNameColor { get; } = ItemColors.Rarity(item.Rarity);

    public string ItemName
    {
        get
        {
            var name = Item.Name;

            if (!Item.Flags.HideSuffix)
            {
                if (!string.IsNullOrEmpty(DefaultSuffixName) && name.EndsWith(DefaultSuffixName!))
                {
                    name = name[..^DefaultSuffixName!.Length];
                    name = name.TrimEnd();
                }

                var newSuffix = SuffixName ?? DefaultSuffixName;
                if (!string.IsNullOrEmpty(newSuffix))
                {
                    name += $" {newSuffix}";
                }
            }

            if (Quantity > 1)
            {
                name = $"{Quantity} {name}";
            }

            return name;
        }
    }

    public string? SuffixName => UpgradesSlots
        .FirstOrDefault(u => u is
        {
            Type: UpgradeSlotType.Default,
            UpgradeComponent: not null
        })
        ?.UpgradeComponent?.SuffixName ?? DefaultSuffixName;

    public Coin TotalVendorValue => Item.VendorValue * Quantity;

    public string AttributeName(Extensible<AttributeName> stat)
    {
        return stat.IsDefined()
            ? stat.ToEnum() switch
            {
                GuildWars2.Hero.AttributeName.Power => "Power",
                GuildWars2.Hero.AttributeName.Precision => "Precision",
                GuildWars2.Hero.AttributeName.Toughness => "Toughness",
                GuildWars2.Hero.AttributeName.Vitality => "Vitality",
                GuildWars2.Hero.AttributeName.Concentration => "Concentration",
                GuildWars2.Hero.AttributeName.ConditionDamage => "Condition Damage",
                GuildWars2.Hero.AttributeName.Expertise => "Expertise",
                GuildWars2.Hero.AttributeName.Ferocity => "Ferocity",
                GuildWars2.Hero.AttributeName.HealingPower => "Healing Power",
                GuildWars2.Hero.AttributeName.AgonyResistance => "Agony Resistance",
                _ => stat.ToString()
            }
            : stat.ToString();
    }

    public AsyncTexture2D? GetIcon(Item item)
    {
        return icons.GetIcon(item);
    }

    public async Task Load(IProgress<string> progress)
    {
        switch (Item)
        {
            case Transmutation transmutation:
                progress.Report("Checking unlock status...");
                await SkinUnlock(transmutation.SkinIds.First());
                break;
            case Armor armor:
                progress.Report("Checking unlock status...");
                await SkinUnlock(armor.DefaultSkinId);
                break;
            case Backpack back:
                progress.Report("Checking unlock status...");
                await SkinUnlock(back.DefaultSkinId);
                break;
            case Weapon weapon:
                progress.Report("Checking unlock status...");
                await SkinUnlock(weapon.DefaultSkinId);
                break;
            case Gizmo gizmo:
                progress.Report("Checking unlock status...");
                await NoveltyUnlock(item.Id);
                break;
        }
    }

    private async Task SkinUnlock(int skinId)
    {
        try
        {
            DefaultLocked = true;

            if (hero.UnlocksAvailable)
            {
                var unlocks = await hero.GetUnlockedWardrobe(CancellationToken.None);
                Unlocked = unlocks.Contains(skinId);
            }
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Couldn't get unlocked wardrobe.");
        }
    }

    private async Task NoveltyUnlock(int itemId)
    {
        try
        {
            var novelties = await hero.GetNovelties(CancellationToken.None);
            var match = novelties.FirstOrDefault(novelty => novelty.UnlockItemIds.Contains(itemId));
            if (match is null)
            {
                return;
            }

            DefaultLocked = true;

            if (hero.UnlocksAvailable)
            {
                var unlocks = await hero.GetUnlockedNovelties(CancellationToken.None);
                Unlocked = unlocks.Contains(match.Id);
            }
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Couldn't get unlocked novelties.");
        }
    }
}