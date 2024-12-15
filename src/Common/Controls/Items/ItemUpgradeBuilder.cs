using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;

using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.Xna.Framework;

using Container = Blish_HUD.Controls.Container;


namespace SL.Common.Controls.Items;

internal sealed class ItemUpgradeBuilder(ItemFlags flags, List<UpgradeComponent> upgrades)
{
    private static readonly Color UpgradeTextColor = new(0x55, 0x99, 0xFF);

    private readonly List<InfusionSlot> _infusionSlots = [];

    private int? _secondarySuffixItemId;

    private int? _suffixItemId;

    private bool _twoHanded;

    public void AddSuffixItem(int? itemId)
    {
        _suffixItemId = itemId;
    }

    public void AddSecondarySuffixItemId(int? itemId)
    {
        _secondarySuffixItemId = itemId;
    }

    public void AddInfusionSlots(IEnumerable<InfusionSlot> slots)
    {
        _infusionSlots.AddRange(slots);
    }

    public void TwoHanded()
    {
        _twoHanded = true;
    }

    public void Build(Container parent)
    {
        if (_suffixItemId.HasValue && !flags.NotUpgradeable)
        {
            UpgradeSlot(_suffixItemId.Value, upgrades, parent);
        }

        if (_twoHanded && _secondarySuffixItemId.HasValue && !flags.NotUpgradeable)
        {
            UpgradeSlot(_secondarySuffixItemId.Value, upgrades, parent);
        }

        foreach (InfusionSlot? slot in _infusionSlots.Where(slot => slot.ItemId.HasValue))
        {
            InfusionSlot(slot, upgrades, parent);
        }

        if (!_suffixItemId.HasValue && !flags.NotUpgradeable)
        {
            UnusedUpgradeSlot(parent);
        }

        if (_twoHanded && !_secondarySuffixItemId.HasValue && !flags.NotUpgradeable)
        {
            UnusedUpgradeSlot(parent);
        }

        foreach (InfusionSlot? slot in _infusionSlots.Where(slot => !slot.ItemId.HasValue))
        {
            UnusedInfusionSlot(slot, parent);
        }
    }

    private static Control UnusedUpgradeSlot(Container parent)
    {
        FormattedLabel? upgrade = new FormattedLabelBuilder()
            .SetWidth(parent.Width)
            .AutoSizeHeight()
            .Wrap()
            .CreatePart("\r\n", _ => { })
            .CreatePart(" Unused Upgrade Slot", part =>
            {
                part.SetPrefixImage(AsyncTexture2D.FromAssetId(517197));
                part.SetPrefixImageSize(new Point(16));
                part.SetFontSize(ContentService.FontSize.Size16);
            })
            .Build();
        upgrade.Parent = parent;
        return upgrade;
    }

    private static Control? UpgradeSlot(int itemId, List<UpgradeComponent> upgrades, Container parent)
    {
        UpgradeComponent? item = upgrades.Find(upgrade => upgrade.Id == itemId);
        if (item is null)
        {
            return UnusedUpgradeSlot(parent);
        }

        FormattedLabelBuilder builder = new FormattedLabelBuilder()
            .SetWidth(parent.Width)
            .AutoSizeHeight()
            .Wrap()
            .CreatePart("\r\n", _ => { })
            .CreatePart(" " + item.Name, part =>
            {
                if (!string.IsNullOrEmpty(item.IconHref))
                {
                    part.SetPrefixImage(GameService.Content.GetRenderServiceTexture(item.IconHref));
                    part.SetPrefixImageSize(new Point(16));
                }

                part.SetFontSize(ContentService.FontSize.Size16);
                part.SetTextColor(UpgradeTextColor);
            });

        if (item is Rune rune)
        {
            foreach ((string? bonus, int ordinal) in (rune.Bonuses ?? []).Select((value, index) => (value, index + 1)))
            {
                builder.CreatePart($"\r\n({ordinal:0}): {bonus}", part =>
                {
                    part.SetFontSize(ContentService.FontSize.Size16);
                    part.SetTextColor(new Color(0x99, 0x99, 0x99));
                });
            }
        }
        else if (item.Buff is { Description.Length: > 0 })
        {
            builder.CreatePart("\r\n", part => part.SetFontSize(ContentService.FontSize.Size16));
            builder.AddMarkup(item.Buff.Description, new Color(0x55, 0x99, 0xFF));
        }
        else
        {
            foreach (KeyValuePair<Extensible<AttributeName>, int> stat in item.Attributes)
            {
                builder.CreatePart("\r\n", part => part.SetFontSize(ContentService.FontSize.Size16));
                builder.CreatePart($"+{stat.Value:N0} {AttributeName(stat.Key)}", part =>
                {
                    part.SetFontSize(ContentService.FontSize.Size16);
                    part.SetTextColor(UpgradeTextColor);
                });
            }
        }

        FormattedLabel? upgrade = builder.Build();
        upgrade.Parent = parent;
        return upgrade;
    }

    private static string AttributeName(Extensible<AttributeName> stat)
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

    private static Control? UnusedInfusionSlot(InfusionSlot slot, Container parent)
    {
        FormattedLabelBuilder builder = new FormattedLabelBuilder()
            .SetWidth(parent.Width)
            .AutoSizeHeight()
            .Wrap()
            .CreatePart("\r\n", _ => { });

        if (slot.Flags.Infusion)
        {
            builder.CreatePart(" Unused Infusion Slot", part =>
            {
                part.SetPrefixImage(AsyncTexture2D.FromAssetId(517202));
                part.SetPrefixImageSize(new Point(16));
                part.SetFontSize(ContentService.FontSize.Size16);
            });
        }
        else if (slot.Flags.Enrichment)
        {
            builder.CreatePart(" Unused Enrichment Slot", part =>
            {
                part.SetPrefixImage(AsyncTexture2D.FromAssetId(517204));
                part.SetPrefixImageSize(new Point(16));
                part.SetFontSize(ContentService.FontSize.Size16);
            });
        }

        FormattedLabel? slotLabel = builder.Build();
        slotLabel.Parent = parent;
        return slotLabel;
    }

    private static Control? InfusionSlot(InfusionSlot slot, List<UpgradeComponent> upgrades, Container parent)
    {
        if (!slot.ItemId.HasValue)
        {
            return UnusedInfusionSlot(slot, parent);
        }

        UpgradeComponent? item = upgrades.Find(upgrade => upgrade.Id == slot.ItemId.Value);
        if (item is null)
        {
            return UnusedInfusionSlot(slot, parent);
        }

        FormattedLabelBuilder builder = new FormattedLabelBuilder()
            .SetWidth(parent.Width)
            .AutoSizeHeight()
            .Wrap()
            .CreatePart("\r\n", _ => { })
            .CreatePart(" " + item.Name, part =>
            {
                if (!string.IsNullOrEmpty(item.IconHref))
                {
                    part.SetPrefixImage(GameService.Content.GetRenderServiceTexture(item.IconHref));
                    part.SetPrefixImageSize(new Point(16));
                }

                part.SetFontSize(ContentService.FontSize.Size16);
                part.SetTextColor(UpgradeTextColor);
            });

        if (!string.IsNullOrEmpty(item.Buff?.Description))
        {
            builder.CreatePart("\r\n", _ => { });
            builder.CreatePart(item.Buff!.Description, part =>
            {
                part.SetFontSize(ContentService.FontSize.Size16);
                part.SetTextColor(UpgradeTextColor);
            });
        }
        else
        {
            foreach (KeyValuePair<Extensible<AttributeName>, int> stat in item.Attributes)
            {
                builder.CreatePart("\r\n", _ => { });
                builder.CreatePart($"+{stat.Value:N0} {AttributeName(stat.Key)}", part =>
                {
                    part.SetFontSize(ContentService.FontSize.Size16);
                    part.SetTextColor(UpgradeTextColor);
                });
            }
        }

        FormattedLabel? slotLabel = builder.Build();
        slotLabel.Parent = parent;
        return slotLabel;
    }
}