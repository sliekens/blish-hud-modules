using Blish_HUD.Content;

using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.Extensions.Localization;
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
    IEnumerable<UpgradeSlot> upgrades,
    IStringLocalizer<ItemTooltipViewModel> localizer
) : ViewModel
{
    private bool? _unlocked;

    private bool _locked;

    private string? _unlockedText;

    private string? _skinName;
    private Color? _unlockedTextColor;

    public IReadOnlyList<UpgradeSlot> UpgradesSlots { get; } = upgrades.ToList();

    public Item Item { get; } = item;

    public bool ContentUnlocksAvailable => hero.UnlocksAvailable;

    public bool DyeUnlocksAvailable => hero.UnlocksAvailable;

    public bool GliderSkinUnlocksAvailable => hero.UnlocksAvailable;

    public bool JadeBotSkinUnlocksAvailable => hero is { UnlocksAvailable: true, InventoriesAvailable: true };

    public bool MistChampionSkinUnlocksAvailable => hero.UnlocksAvailable;

    public bool NoveltyUnlocksAvailable => hero.UnlocksAvailable;

    public bool OutfitUnlocksAvailable => hero.UnlocksAvailable;

    public bool RecipeUnlocksAvailable => hero.UnlocksAvailable;

    public bool WardrobeUnlocksAvailable => hero.UnlocksAvailable;

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

    public string? UnlockedText
    {
        get => _unlockedText;
        set => SetField(ref _unlockedText, value);
    }

    public Color? UnlockedTextColor
    {
        get => _unlockedTextColor;
        set => SetField(ref _unlockedTextColor, value);
    }

    public string? SkinName
    {
        get => _skinName;
        set => SetField(ref _skinName, value);
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

    public string? AuthorizationText { get; private set; }

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
            case ContentUnlocker unlocker:
                progress.Report("Checking unlock status...");
                try
                {
                    var finishers = await hero.GetFinishers(CancellationToken.None);
                    var finisher = finishers.FirstOrDefault(finisher => finisher.UnlockItemIds.Contains(unlocker.Id));
                    if (finisher is not null)
                    {
                        if (ContentUnlocksAvailable)
                        {
                            var unlocks = await hero.GetUnlockedFinishers(CancellationToken.None);
                            Unlocked = unlocks.Contains(finisher.Id);
                        }
                        else
                        {
                            AuthorizationText =
                                "Grant 'unlocks' permission in settings to see unlock status";
                        }

                        DefaultLocked = true;
                        break;
                    }

                    var mailCarriers = await hero.GetMailCarriers(CancellationToken.None);
                    var mailCarrier = mailCarriers.FirstOrDefault(mailCarrier => mailCarrier.UnlockItemIds.Contains(unlocker.Id));
                    if (mailCarrier is not null)
                    {
                        if (hero.UnlocksAvailable)
                        {
                            var unlocks = await hero.GetUnlockedMailCarriers(CancellationToken.None);
                            Unlocked = unlocks.Contains(mailCarrier.Id);
                        }
                        else
                        {
                            AuthorizationText =
                                "Grant 'unlocks' permission in settings to see unlock status";
                        }

                        DefaultLocked = true;
                        break;
                    }
                }
                catch (Exception reason)
                {
                    logger.LogWarning(reason, "Couldn't get unlocks.");
                }

                break;
            case Dye unlocker:
                progress.Report("Checking unlock status...");
                try
                {
                    var dyes = await hero.GetDyes(CancellationToken.None);
                    var dye = dyes.FirstOrDefault(dye => dye.ItemId == unlocker.Id);
                    if (dye is not null)
                    {
                        if (DyeUnlocksAvailable)
                        {
                            var unlocks = await hero.GetUnlockedDyes(CancellationToken.None);
                            Unlocked = unlocks.Contains(dye.Id);
                        }
                        else
                        {
                            AuthorizationText =
                                "Grant 'unlocks' permission in settings to see unlock status";
                        }

                        DefaultLocked = true;
                    }
                }
                catch (Exception reason)
                {
                    logger.LogWarning(reason, "Couldn't get unlocks.");
                }

                break;
            case GliderSkinUnlocker unlocker:
                progress.Report("Checking unlock status...");
                try
                {
                    var gliderSkins = await hero.GetGliderSkins(CancellationToken.None);
                    var gliderSkin = gliderSkins.FirstOrDefault(gliderSkin => gliderSkin.UnlockItemIds.Contains(unlocker.Id));
                    if (gliderSkin is not null)
                    {
                        if (GliderSkinUnlocksAvailable)
                        {
                            var unlocks = await hero.GetUnlockedGliderSkins(CancellationToken.None);
                            Unlocked = unlocks.Contains(gliderSkin.Id);
                        }
                        else
                        {
                            AuthorizationText =
                                "Grant 'unlocks' permission in settings to see unlock status";
                        }

                        DefaultLocked = true;
                    }
                }
                catch (Exception reason)
                {
                    logger.LogWarning(reason, "Couldn't get unlocks.");
                }

                break;
            case JadeBotSkinUnlocker unlocker:
                progress.Report("Checking unlock status...");
                try
                {
                    var jadeBotSkins = await hero.GetJadeBotSkins(CancellationToken.None);
                    var jadeBotSkin = jadeBotSkins.FirstOrDefault(jadeBotSkin => jadeBotSkin.UnlockItemId == unlocker.Id);
                    if (jadeBotSkin is not null)
                    {
                        if (JadeBotSkinUnlocksAvailable)
                        {
                            var unlocks = await hero.GetUnlockedJadeBotSkins(CancellationToken.None);
                            Unlocked = unlocks.Contains(jadeBotSkin.Id);
                        }
                        else
                        {
                            AuthorizationText =
                                "Grant 'unlocks' and 'inventories' permission in settings to see unlock status";
                        }

                        DefaultLocked = true;
                    }
                }
                catch (Exception reason)
                {
                    logger.LogWarning(reason, "Couldn't get unlocks.");
                }
                break;
            case OutfitUnlocker unlocker:
                progress.Report("Checking unlock status...");
                try
                {
                    var outfits = await hero.GetOutfits(CancellationToken.None);
                    var outfit = outfits.FirstOrDefault(outfit => outfit.UnlockItemIds.Contains(unlocker.Id));
                    if (outfit is not null)
                    {
                        if (OutfitUnlocksAvailable)
                        {
                            var unlocks = await hero.GetUnlockedOutfits(CancellationToken.None);
                            Unlocked = unlocks.Contains(outfit.Id);
                        }
                        else
                        {
                            AuthorizationText =
                                "Grant 'unlocks' permission in settings to see unlock status";
                        }

                        DefaultLocked = true;
                        break;
                    }
                }
                catch (Exception reason)
                {
                    logger.LogWarning(reason, "Couldn't get unlocks.");
                }

                break;
            case MistChampionSkinUnlocker unlocker:
                progress.Report("Checking unlock status...");
                try
                {
                    var mistChampionSkins = await hero.GetMistChampionSkins(CancellationToken.None);
                    var mistChampionSkin = mistChampionSkins.FirstOrDefault(mistChampionSkin => mistChampionSkin.UnlockItemIds.Contains(unlocker.Id));
                    if (mistChampionSkin is not null)
                    {
                        if (MistChampionSkinUnlocksAvailable)
                        {
                            var unlocks = await hero.GetUnlockedMistChampionSkins(CancellationToken.None);
                            Unlocked = unlocks.Contains(mistChampionSkin.Id);
                        }
                        else
                        {
                            AuthorizationText =
                                "Grant 'unlocks' permission in settings to see unlock status";
                        }

                        DefaultLocked = true;
                        break;
                    }
                }
                catch (Exception reason)
                {
                    logger.LogWarning(reason, "Couldn't get unlocks.");
                }

                break;
            case RecipeSheet unlocker:
                progress.Report("Checking unlock status...");
                try
                {
                    if (RecipeUnlocksAvailable)
                    {
                        var unlocks = await hero.GetUnlockedRecipes(CancellationToken.None);
                        if (unlocks.Contains(unlocker.RecipeId))
                        {
                            Unlocked = true;
                            UnlockedText = unlocker.ExtraRecipeIds.Any()
                                ? localizer["All recipes unlocked"]
                                : localizer["Recipe unlocked"];
                            UnlockedTextColor = Color.Red;
                        }
                        else if (unlocker.ExtraRecipeIds.Any(unlocks.Contains))
                        {
                            Unlocked = true;
                            var known = unlocker.ExtraRecipeIds.Count(unlocks.Contains);
                            UnlockedText = localizer["Recipes unlocked", known, unlocker.ExtraRecipeIds.Count + 1];
                            UnlockedTextColor = Color.Yellow;
                        }
                        else
                        {
                            Unlocked = false;
                        }
                    }
                    else
                    {
                        AuthorizationText = localizer["Grant unlocks permission"];
                    }

                    DefaultLocked = true;
                    break;
                }
                catch (Exception reason)
                {
                    logger.LogWarning(reason, "Couldn't get unlocks.");
                }

                break;
            case Unlocker other:
                logger.LogInformation("No handling for: {@Other}", other);
                break;
            case Gizmo gizmo:
                progress.Report("Checking unlock status...");
                var novelties = await hero.GetNovelties(CancellationToken.None);
                var novelty = novelties.FirstOrDefault(novelty => novelty.UnlockItemIds.Contains(gizmo.Id));
                if (novelty is not null)
                {
                    if (NoveltyUnlocksAvailable)
                    {
                        var unlocks = await hero.GetUnlockedNovelties(CancellationToken.None);
                        Unlocked = unlocks.Contains(novelty.Id);
                    }
                    else
                    {
                        AuthorizationText =
                            "Grant 'unlocks' permission in settings to see unlock status";
                    }

                    DefaultLocked = true;
                }

                break;
        }
    }

    private async Task SkinUnlock(int skinId)
    {
        try
        {
            DefaultLocked = true;

            var wardrobe = await hero.GetWardrobe(CancellationToken.None);
            var skin = wardrobe.FirstOrDefault(skin => skin.Id == skinId);
            if (skin is not null)
            {
                SkinName = skin.Name;
            }

            if (WardrobeUnlocksAvailable)
            {
                var unlocks = await hero.GetUnlockedWardrobe(CancellationToken.None);
                Unlocked = unlocks.Contains(skinId);
            }
            else
            {
                AuthorizationText = "Grant 'unlocks' permission in settings to see unlock status";
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