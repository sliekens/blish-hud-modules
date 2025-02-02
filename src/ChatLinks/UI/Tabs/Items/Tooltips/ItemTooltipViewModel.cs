﻿using System.Globalization;

using Blish_HUD.Content;

using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Hero.Equipment.Wardrobe;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

using SL.ChatLinks.Storage;
using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items.Tooltips;

public sealed class ItemTooltipViewModel(
    ILogger<ItemTooltipViewModel> logger,
    IDbContextFactory contextFactory,
    ILocale locale,
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

    private Color? _unlockedTextColor;

    private EquipmentSkin? _skin;

    public IReadOnlyList<UpgradeSlot> UpgradesSlots { get; } = upgrades.ToList();

    public Item Item { get; } = item;

    public EquipmentSkin? DefaultSkin
    {
        get => _skin;
        private set => SetField(ref _skin, value);
    }

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
                await GetSkin(transmutation.SkinIds.First());
                break;
            case Armor armor:
                progress.Report("Checking unlock status...");
                await GetSkin(armor.DefaultSkinId);
                break;
            case Backpack back:
                progress.Report("Checking unlock status...");
                await GetSkin(back.DefaultSkinId);
                break;
            case Weapon weapon:
                progress.Report("Checking unlock status...");
                await GetSkin(weapon.DefaultSkinId);
                break;
            case ContentUnlocker unlocker:
                progress.Report("Checking unlock status...");
                try
                {
                    await using var context = contextFactory.CreateDbContext(locale.Current);

                    var finisher = await context.Finishers
                        .FromSqlInterpolated($"""
                                              SELECT *
                                              FROM Finishers, json_each(Finishers.UnlockItemIds)
                                              WHERE json_each.value = {unlocker.Id}
                                              """)
                        .FirstOrDefaultAsync();
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

                    var mailCarrier = await context.MailCarrriers
                        .FromSqlInterpolated($"""
                                              SELECT *
                                              FROM MailCarriers, json_each(MailCarriers.UnlockItemIds)
                                              WHERE json_each.value = {unlocker.Id}
                                              """)
                        .FirstOrDefaultAsync();
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
                    await using var context = contextFactory.CreateDbContext(locale.Current);
                    var dye = context.Colors.FirstOrDefault(dye => dye.ItemId == unlocker.Id);
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
                    await using var context = contextFactory.CreateDbContext(locale.Current);

                    var gliderSkin = await context.Gliders
                        .FromSqlInterpolated($"""
                                              SELECT *
                                              FROM Gliders, json_each(Gliders.UnlockItemIds)
                                              WHERE json_each.value = {unlocker.Id}
                                              """)
                        .FirstOrDefaultAsync();
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
                    await using var context = contextFactory.CreateDbContext(locale.Current);

                    var jadeBotSkin =
                        context.JadeBots.FirstOrDefault(jadeBotSkin => jadeBotSkin.UnlockItemId == unlocker.Id);
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
                    await using var context = contextFactory.CreateDbContext(locale.Current);

                    var outfit = await context.Outfits
                        .FromSqlInterpolated($"""
                                              SELECT *
                                              FROM Outfits, json_each(Outfits.UnlockItemIds)
                                              WHERE json_each.value = {unlocker.Id}
                                              """)
                        .FirstOrDefaultAsync();
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
                    await using var context = contextFactory.CreateDbContext(locale.Current);

                    var mistChampionSkin = await context.MistChampions
                        .FromSqlInterpolated($"""
                                              SELECT *
                                              FROM MistChampions, json_each(MistChampions.UnlockItemIds)
                                              WHERE json_each.value = {unlocker.Id}
                                              """)
                        .FirstOrDefaultAsync();
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
                try
                {
                    await using var context = contextFactory.CreateDbContext(locale.Current);

                    var novelty = await context.Novelties
                        .FromSqlInterpolated($"""
                                              SELECT *
                                              FROM Novelties, json_each(Novelties.UnlockItemIds)
                                              WHERE json_each.value = {gizmo.Id}
                                              """)
                        .FirstOrDefaultAsync();
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
                }
                catch (Exception reason)
                {
                    logger.LogWarning(reason, "Couldn't get unlocks.");
                }

                break;
        }
    }

    private async Task GetSkin(int skinId)
    {
        try
        {
            DefaultLocked = true;

            await using var context = contextFactory.CreateDbContext(locale.Current);
            DefaultSkin = await context.Skins.FirstOrDefaultAsync(skin => skin.Id == skinId);

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
}