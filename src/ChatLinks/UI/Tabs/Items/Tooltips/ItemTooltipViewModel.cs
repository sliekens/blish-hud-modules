using Blish_HUD.Content;

using GuildWars2;
using GuildWars2.Authorization;
using GuildWars2.Hero;
using GuildWars2.Hero.Equipment.Wardrobe;
using GuildWars2.Items;

using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items.Tooltips;

public sealed class ItemTooltipViewModel(
    ILogger<ItemTooltipViewModel> logger,
    Gw2Client gw2Client,
    ITokenProvider tokenProvider,
    ItemIcons icons,
    Customizer customizer,
    Item item,
    int quantity,
    IEnumerable<UpgradeSlot> upgrades
) : ViewModel
{
    private EquipmentSkin? _skin;
    private bool? _skinUnlocked;

    public IReadOnlyList<UpgradeSlot> UpgradesSlots { get; } = upgrades.ToList();

    public Item Item { get; } = item;

    public EquipmentSkin? Skin
    {
        get => _skin;
        private set => SetField(ref _skin, value);
    }

    public bool? SkinUnlocked
    {
        get => _skinUnlocked;
        set => SetField(ref _skinUnlocked, value);
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
                await LoadSkin(transmutation.SkinIds.First());
                break;
            case Armor armor:
                progress.Report("Checking unlock status...");
                await LoadSkin(armor.DefaultSkinId);
                break;
            case Backpack back:
                progress.Report("Checking unlock status...");
                await LoadSkin(back.DefaultSkinId);
                break;
            case Weapon weapon:
                progress.Report("Checking unlock status...");
                await LoadSkin(weapon.DefaultSkinId);
                break;
        }
    }

    private async Task LoadSkin(int skinId)
    {
        Skin = await gw2Client.Hero.Equipment.Wardrobe
            .GetSkinById(skinId)
            .ValueOnly();

        try
        {
            if (tokenProvider.Grants.Contains(Permission.Unlocks))
            {
                var token = await tokenProvider.GetTokenAsync(CancellationToken.None);
                var unlock = await gw2Client.Hero.Equipment.Wardrobe.GetUnlockedSkins(token).ValueOnly();
                SkinUnlocked = unlock.Contains(skinId);
            }
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Couldn't get wardrobe unlocks.");
        }
    }
}