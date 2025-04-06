using Blish_HUD.Content;

using GuildWars2;
using GuildWars2.Authorization;
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

using Miniature = GuildWars2.Hero.Equipment.Miniatures.Miniature;
using MiniatureItem = GuildWars2.Items.Miniature;

namespace SL.ChatLinks.UI.Tabs.Items.Tooltips;

public sealed class ItemTooltipViewModel(
    ILogger<ItemTooltipViewModel> logger,
    IDbContextFactory contextFactory,
    ILocale locale,
    IconsService icons,
    Customizer customizer,
    AccountUnlocks unlocks,
    Item item,
    int quantity,
    IEnumerable<UpgradeSlot> upgrades,
    IStringLocalizer<ItemTooltipView> localizer
) : ViewModel
{
    public delegate ItemTooltipViewModel Factory(Item item, int quantity, IEnumerable<UpgradeSlot> upgrades);

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
            Type: UpgradeSlotType.Default or UpgradeSlotType.Banana,
            UpgradeComponent: not null
        })
        ?.UpgradeComponent?.SuffixName ?? DefaultSuffixName;

    public Coin TotalVendorValue => Item.VendorValue * Quantity;

    public string? AuthorizationText { get; private set; }

    public AsyncTexture2D? GetIcon(Item item)
    {
        return icons.GetIcon(item.IconUrl());
    }

    public AsyncTexture2D? GetIcon(Uri? iconUrl)
    {
        return icons.GetIcon(iconUrl);
    }

#pragma warning disable CA1822 // Mark members as static
    public AsyncTexture2D? GetIcon(int assetId)
    {
        return AsyncTexture2D.FromAssetId(assetId).Duplicate();
    }
#pragma warning restore CA1822

    public async Task Load(IProgress<string> progress)
    {
        ThrowHelper.ThrowIfNull(progress);
        switch (Item)
        {
            case Transmutation transmutation:
                progress.Report("Checking unlock status...");
                await GetSkin(transmutation.SkinIds.First()).ConfigureAwait(false);
                break;
            case Armor armor:
                progress.Report("Checking unlock status...");
                await GetSkin(armor.DefaultSkinId).ConfigureAwait(false);
                break;
            case Backpack back:
                progress.Report("Checking unlock status...");
                await GetSkin(back.DefaultSkinId).ConfigureAwait(false);
                break;
            case Weapon weapon:
                progress.Report("Checking unlock status...");
                await GetSkin(weapon.DefaultSkinId).ConfigureAwait(false);
                break;
            case ContentUnlocker unlocker:
                progress.Report("Checking unlock status...");
                try
                {
                    ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
                    await using (context.ConfigureAwait(false))
                    {
                        Finisher? finisher = await context.Finishers
                            .FromSqlInterpolated($"""
                                              SELECT *
                                              FROM Finishers, json_each(Finishers.UnlockItemIds)
                                              WHERE json_each.value = {unlocker.Id}
                                              """)
                            .FirstOrDefaultAsync().ConfigureAwait(false);
                        if (finisher is not null)
                        {
                            if (unlocks.HasPermission(Permission.Unlocks))
                            {
                                IReadOnlyList<int> unlocked = await unlocks.GetUnlockedFinishers(CancellationToken.None).ConfigureAwait(false);
                                Unlocked = unlocked.Contains(finisher.Id);
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
                            .FirstOrDefaultAsync().ConfigureAwait(false);
                        if (mailCarrier is not null)
                        {
                            if (unlocks.HasPermission(Permission.Unlocks))
                            {
                                IReadOnlyList<int> unlocked = await unlocks.GetUnlockedMailCarriers(CancellationToken.None).ConfigureAwait(false);
                                Unlocked = unlocked.Contains(mailCarrier.Id);
                            }
                            else
                            {
                                AuthorizationText = Localizer["Grant unlocks permission"];

                            }

                            DefaultLocked = true;
                            break;
                        }
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
                    ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
                    await using (context.ConfigureAwait(false))
                    {
                        DyeColor? dye = context.Colors.FirstOrDefault(dye => dye.ItemId == unlocker.Id);
                        if (dye is not null)
                        {
                            if (unlocks.HasPermission(Permission.Unlocks))
                            {
                                IReadOnlyList<int> unlocked = await unlocks.GetUnlockedDyes(CancellationToken.None).ConfigureAwait(false);
                                Unlocked = unlocked.Contains(dye.Id);
                            }
                            else
                            {
                                AuthorizationText = Localizer["Grant unlocks permission"];
                            }

                            DefaultLocked = true;
                        }
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
                    ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
                    await using (context.ConfigureAwait(false))
                    {
                        GliderSkin? gliderSkin = await context.Gliders
                            .FromSqlInterpolated($"""
                                              SELECT *
                                              FROM Gliders, json_each(Gliders.UnlockItemIds)
                                              WHERE json_each.value = {unlocker.Id}
                                              """)
                            .FirstOrDefaultAsync().ConfigureAwait(false);
                        if (gliderSkin is not null)
                        {
                            if (unlocks.HasPermission(Permission.Unlocks))
                            {
                                IReadOnlyList<int> unlocked = await unlocks.GetUnlockedGliderSkins(CancellationToken.None).ConfigureAwait(false);
                                Unlocked = unlocked.Contains(gliderSkin.Id);
                            }
                            else
                            {
                                AuthorizationText = Localizer["Grant unlocks permission"];
                            }

                            DefaultLocked = true;
                        }
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
                    ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
                    await using (context.ConfigureAwait(false))
                    {
                        JadeBotSkin? jadeBotSkin =
                            context.JadeBots.FirstOrDefault(jadeBotSkin => jadeBotSkin.UnlockItemId == unlocker.Id);
                        if (jadeBotSkin is not null)
                        {
                            if (unlocks.HasPermissions(Permission.Unlocks, Permission.Inventories))
                            {
                                IReadOnlyList<int> unlocked = await unlocks.GetUnlockedJadeBotSkins(CancellationToken.None).ConfigureAwait(false);
                                Unlocked = unlocked.Contains(jadeBotSkin.Id);
                            }
                            else
                            {
                                AuthorizationText = Localizer["Grant unlocks and inventories permission"];
                            }

                            DefaultLocked = true;
                        }
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
                    ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
                    await using (context.ConfigureAwait(false))
                    {
                        Miniature? miniature =
                            await context.Miniatures.FirstOrDefaultAsync(miniature => miniature.ItemId == Item.Id).ConfigureAwait(false);
                        if (miniature is not null)
                        {
                            if (unlocks.HasPermission(Permission.Unlocks))
                            {
                                IReadOnlyList<int> unlocked = await unlocks.GetUnlockedMiniatures(CancellationToken.None).ConfigureAwait(false);
                                Unlocked = unlocked.Contains(miniature.Id);
                            }
                            else
                            {
                                AuthorizationText = Localizer["Grant unlocks permission"];
                            }

                            DefaultLocked = true;
                            break;
                        }
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
                    ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
                    await using (context.ConfigureAwait(false))
                    {
                        Outfit? outfit = await context.Outfits
                            .FromSqlInterpolated($"""
                                              SELECT *
                                              FROM Outfits, json_each(Outfits.UnlockItemIds)
                                              WHERE json_each.value = {unlocker.Id}
                                              """)
                            .FirstOrDefaultAsync().ConfigureAwait(false);
                        if (outfit is not null)
                        {
                            if (unlocks.HasPermission(Permission.Unlocks))
                            {
                                IReadOnlyList<int> unlocked = await unlocks.GetUnlockedOutfits(CancellationToken.None).ConfigureAwait(false);
                                Unlocked = unlocked.Contains(outfit.Id);
                            }
                            else
                            {
                                AuthorizationText = Localizer["Grant unlocks permission"];
                            }

                            DefaultLocked = true;
                            break;
                        }
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
                    ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
                    await using (context.ConfigureAwait(false))
                    {
                        MistChampionSkin? mistChampionSkin = await context.MistChampions
                            .FromSqlInterpolated($"""
                                              SELECT *
                                              FROM MistChampions, json_each(MistChampions.UnlockItemIds)
                                              WHERE json_each.value = {unlocker.Id}
                                              """)
                            .FirstOrDefaultAsync().ConfigureAwait(false);
                        if (mistChampionSkin is not null)
                        {
                            if (unlocks.HasPermission(Permission.Unlocks))
                            {
                                IReadOnlyList<int> unlocked = await unlocks.GetUnlockedMistChampionSkins(CancellationToken.None).ConfigureAwait(false);
                                Unlocked = unlocked.Contains(mistChampionSkin.Id);
                            }
                            else
                            {
                                AuthorizationText = Localizer["Grant unlocks permission"];
                            }

                            DefaultLocked = true;
                            break;
                        }
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
                    if (unlocks.HasPermission(Permission.Unlocks))
                    {
                        IReadOnlyList<int> unlocked = await unlocks.GetUnlockedRecipes(CancellationToken.None).ConfigureAwait(false);
                        if (unlocked.Contains(unlocker.RecipeId))
                        {
                            Unlocked = true;
                            UnlockedText = unlocker.ExtraRecipeIds.Count != 0
                                ? Localizer["All recipes unlocked"]
                                : Localizer["Recipe unlocked"];
                            UnlockedTextColor = Color.Red;
                        }
                        else if (unlocker.ExtraRecipeIds.Any(unlocked.Contains))
                        {
                            Unlocked = true;
                            int known = unlocker.ExtraRecipeIds.Count(unlocked.Contains);
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
                    ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
                    await using (context.ConfigureAwait(false))
                    {
                        Novelty? novelty = await context.Novelties
                            .FromSqlInterpolated($"""
                                              SELECT *
                                              FROM Novelties, json_each(Novelties.UnlockItemIds)
                                              WHERE json_each.value = {gizmo.Id}
                                              """)
                            .FirstOrDefaultAsync().ConfigureAwait(false);
                        if (novelty is not null)
                        {
                            if (unlocks.HasPermission(Permission.Unlocks))
                            {
                                IReadOnlyList<int> unlocked = await unlocks.GetUnlockedNovelties(CancellationToken.None).ConfigureAwait(false);
                                Unlocked = unlocked.Contains(novelty.Id);
                            }
                            else
                            {
                                AuthorizationText = Localizer["Grant unlocks permission"];
                            }

                            DefaultLocked = true;
                        }
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

            ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
            await using (context.ConfigureAwait(false))
            {
                DefaultSkin = await context.Skins.FirstOrDefaultAsync(skin => skin.Id == skinId).ConfigureAwait(false);

                if (unlocks.HasPermission(Permission.Unlocks))
                {
                    IReadOnlyList<int> unlocked = await unlocks.GetUnlockedWardrobe(CancellationToken.None).ConfigureAwait(false);
                    Unlocked = unlocked.Contains(skinId);
                }
                else
                {
                    AuthorizationText = Localizer["Grant unlocks permission"];
                }
            }
        }
        catch (Exception reason)
        {
            logger.LogWarning(reason, "Couldn't get unlocked wardrobe.");
        }
    }
}
