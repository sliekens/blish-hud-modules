using Blish_HUD.Content;

using GuildWars2;
using GuildWars2.Hero.Equipment.Dyes;
using GuildWars2.Hero.Equipment.Finishers;
using GuildWars2.Hero.Equipment.Gliders;
using GuildWars2.Hero.Equipment.JadeBots;
using GuildWars2.Hero.Equipment.MailCarriers;
using GuildWars2.Hero.Equipment.Novelties;
using GuildWars2.Hero.Equipment.Outfits;
using GuildWars2.Hero.Equipment.Wardrobe;
using GuildWars2.Items;
using GuildWars2.Pvp.MistChampions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

using SL.ChatLinks.Storage;
using SL.Common;

using Miniature = GuildWars2.Hero.Equipment.Miniatures.Miniature;
using MiniatureItem = GuildWars2.Items.Miniature;

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
    IStringLocalizer<ItemTooltipView> localizer
) : ViewModel
{
    private bool? _unlocked;

    private bool _locked;

    private string? _unlockedText;

    private Color? _unlockedTextColor;

    private EquipmentSkin? _skin;

    public IReadOnlyList<UpgradeSlot> UpgradesSlots { get; } = [.. upgrades];

    public Item Item { get; } = item;

    public EquipmentSkin? DefaultSkin
    {
        get => _skin;
        private set => SetField(ref _skin, value);
    }

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

    public IStringLocalizer<ItemTooltipView> Localizer { get; } = localizer;

    public string? DefaultSuffixName { get; } = customizer.DefaultSuffixItem(item)?.SuffixName;

    public Color ItemNameColor { get; } = ItemColors.Rarity(item.Rarity);

    public string ItemName
    {
        get
        {
            string name = Item.Name;

            if (!Item.Flags.HideSuffix)
            {
                if (!string.IsNullOrEmpty(DefaultSuffixName) && name.EndsWith(DefaultSuffixName!, StringComparison.Ordinal))
                {
                    name = name[..^DefaultSuffixName!.Length];
                    name = name.TrimEnd();
                }

                string? newSuffix = SuffixName ?? DefaultSuffixName;
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
                    await using ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);

                    Finisher? finisher = await context.Finishers
                        .FromSqlInterpolated($"""
                                              SELECT *
                                              FROM Finishers, json_each(Finishers.UnlockItemIds)
                                              WHERE json_each.value = {unlocker.Id}
                                              """)
                        .FirstOrDefaultAsync();
                    if (finisher is not null)
                    {
                        if (hero.UnlocksAvailable)
                        {
                            IReadOnlyList<int> unlocks = await hero.GetUnlockedFinishers(CancellationToken.None);
                            Unlocked = unlocks.Contains(finisher.Id);
                        }
                        else
                        {
                            AuthorizationText = Localizer["Grant unlocks permission"];
                        }

                        DefaultLocked = true;
                        break;
                    }

                    MailCarrier? mailCarrier = await context.MailCarrriers
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
                            IReadOnlyList<int> unlocks = await hero.GetUnlockedMailCarriers(CancellationToken.None);
                            Unlocked = unlocks.Contains(mailCarrier.Id);
                        }
                        else
                        {
                            AuthorizationText = Localizer["Grant unlocks permission"];

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
                    await using ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
                    DyeColor? dye = context.Colors.FirstOrDefault(dye => dye.ItemId == unlocker.Id);
                    if (dye is not null)
                    {
                        if (hero.UnlocksAvailable)
                        {
                            IReadOnlyList<int> unlocks = await hero.GetUnlockedDyes(CancellationToken.None);
                            Unlocked = unlocks.Contains(dye.Id);
                        }
                        else
                        {
                            AuthorizationText = Localizer["Grant unlocks permission"];
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
                    await using ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);

                    GliderSkin? gliderSkin = await context.Gliders
                        .FromSqlInterpolated($"""
                                              SELECT *
                                              FROM Gliders, json_each(Gliders.UnlockItemIds)
                                              WHERE json_each.value = {unlocker.Id}
                                              """)
                        .FirstOrDefaultAsync();
                    if (gliderSkin is not null)
                    {
                        if (hero.UnlocksAvailable)
                        {
                            IReadOnlyList<int> unlocks = await hero.GetUnlockedGliderSkins(CancellationToken.None);
                            Unlocked = unlocks.Contains(gliderSkin.Id);
                        }
                        else
                        {
                            AuthorizationText = Localizer["Grant unlocks permission"];
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
                    await using ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);

                    JadeBotSkin? jadeBotSkin =
                        context.JadeBots.FirstOrDefault(jadeBotSkin => jadeBotSkin.UnlockItemId == unlocker.Id);
                    if (jadeBotSkin is not null)
                    {
                        if (hero.UnlocksAvailable && hero.InventoriesAvailable)
                        {
                            IReadOnlyList<int> unlocks = await hero.GetUnlockedJadeBotSkins(CancellationToken.None);
                            Unlocked = unlocks.Contains(jadeBotSkin.Id);
                        }
                        else
                        {
                            AuthorizationText = Localizer["Grant unlocks and inventories permission"];
                        }

                        DefaultLocked = true;
                    }
                }
                catch (Exception reason)
                {
                    logger.LogWarning(reason, "Couldn't get unlocks.");
                }

                break;
            case MiniatureItem:
            case MiniatureUnlocker:
                progress.Report("Checking unlock status...");
                try
                {
                    await using ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
                    Miniature? miniature =
                        await context.Miniatures.FirstOrDefaultAsync(miniature => miniature.ItemId == Item.Id);
                    if (miniature is not null)
                    {
                        if (hero.UnlocksAvailable)
                        {
                            IReadOnlyList<int> unlocks = await hero.GetUnlockedMiniatures(CancellationToken.None);
                            Unlocked = unlocks.Contains(miniature.Id);
                        }
                        else
                        {
                            AuthorizationText = Localizer["Grant unlocks permission"];
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
            case OutfitUnlocker unlocker:
                progress.Report("Checking unlock status...");
                try
                {
                    await using ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);

                    Outfit? outfit = await context.Outfits
                        .FromSqlInterpolated($"""
                                              SELECT *
                                              FROM Outfits, json_each(Outfits.UnlockItemIds)
                                              WHERE json_each.value = {unlocker.Id}
                                              """)
                        .FirstOrDefaultAsync();
                    if (outfit is not null)
                    {
                        if (hero.UnlocksAvailable)
                        {
                            IReadOnlyList<int> unlocks = await hero.GetUnlockedOutfits(CancellationToken.None);
                            Unlocked = unlocks.Contains(outfit.Id);
                        }
                        else
                        {
                            AuthorizationText = Localizer["Grant unlocks permission"];
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
                    await using ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);

                    MistChampionSkin? mistChampionSkin = await context.MistChampions
                        .FromSqlInterpolated($"""
                                              SELECT *
                                              FROM MistChampions, json_each(MistChampions.UnlockItemIds)
                                              WHERE json_each.value = {unlocker.Id}
                                              """)
                        .FirstOrDefaultAsync();
                    if (mistChampionSkin is not null)
                    {
                        if (hero.UnlocksAvailable)
                        {
                            IReadOnlyList<int> unlocks = await hero.GetUnlockedMistChampionSkins(CancellationToken.None);
                            Unlocked = unlocks.Contains(mistChampionSkin.Id);
                        }
                        else
                        {
                            AuthorizationText = Localizer["Grant unlocks permission"];
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
                    if (hero.UnlocksAvailable)
                    {
                        IReadOnlyList<int> unlocks = await hero.GetUnlockedRecipes(CancellationToken.None);
                        if (unlocks.Contains(unlocker.RecipeId))
                        {
                            Unlocked = true;
                            UnlockedText = unlocker.ExtraRecipeIds.Count != 0
                                ? Localizer["All recipes unlocked"]
                                : Localizer["Recipe unlocked"];
                            UnlockedTextColor = Color.Red;
                        }
                        else if (unlocker.ExtraRecipeIds.Any(unlocks.Contains))
                        {
                            Unlocked = true;
                            int known = unlocker.ExtraRecipeIds.Count(unlocks.Contains);
                            UnlockedText = Localizer["Recipes unlocked", known, unlocker.ExtraRecipeIds.Count + 1];
                            UnlockedTextColor = Color.Yellow;
                        }
                        else
                        {
                            Unlocked = false;
                        }
                    }
                    else
                    {
                        AuthorizationText = Localizer["Grant unlocks permission"];
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
                    await using ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);

                    Novelty? novelty = await context.Novelties
                        .FromSqlInterpolated($"""
                                              SELECT *
                                              FROM Novelties, json_each(Novelties.UnlockItemIds)
                                              WHERE json_each.value = {gizmo.Id}
                                              """)
                        .FirstOrDefaultAsync();
                    if (novelty is not null)
                    {
                        if (hero.UnlocksAvailable)
                        {
                            IReadOnlyList<int> unlocks = await hero.GetUnlockedNovelties(CancellationToken.None);
                            Unlocked = unlocks.Contains(novelty.Id);
                        }
                        else
                        {
                            AuthorizationText = Localizer["Grant unlocks permission"];
                        }

                        DefaultLocked = true;
                    }
                }
                catch (Exception reason)
                {
                    logger.LogWarning(reason, "Couldn't get unlocks.");
                }

                break;
            default:
                break;
        }
    }

    private async Task GetSkin(int skinId)
    {
        try
        {
            DefaultLocked = true;

            await using ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
            DefaultSkin = await context.Skins.FirstOrDefaultAsync(skin => skin.Id == skinId);

            if (hero.UnlocksAvailable)
            {
                IReadOnlyList<int> unlocks = await hero.GetUnlockedWardrobe(CancellationToken.None);
                Unlocked = unlocks.Contains(skinId);
            }
            else
            {
                AuthorizationText = Localizer["Grant unlocks permission"];
            }
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Couldn't get unlocked wardrobe.");
        }
    }
}
