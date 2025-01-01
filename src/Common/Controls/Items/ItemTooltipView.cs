using System.Data;
using System.Text;

using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.Common.Controls.Items.Services;

using Color = Microsoft.Xna.Framework.Color;
using Container = Blish_HUD.Controls.Container;
using Currency = GuildWars2.Items.Currency;
using Item = GuildWars2.Items.Item;

namespace SL.Common.Controls.Items;

public class ItemTooltipView : View<ItemTooltipPresenter>, IItemTooltipView
{
    private readonly FlowPanel _layout;

    public ItemTooltipView(Item item, IReadOnlyDictionary<int, UpgradeComponent> upgrades)
    {
        this.AutoWire();
        Presenter.Item = item;
        Presenter.Upgrades = upgrades;
        _layout = new()
        {
            FlowDirection = ControlFlowDirection.SingleTopToBottom,
            Width = 350,
            HeightSizingMode = SizingMode.AutoSize,
        };
    }

    public void PrintPlainText(string text, Color? textColor = null)
    {
        _ = new Label
        {
            Parent = _layout,
            Width = _layout.Width,
            AutoSizeHeight = true,
            Text = text,
            TextColor = textColor.GetValueOrDefault(Color.White),
            Font = GameService.Content.DefaultFont16,
            WrapText = true
        };
    }

    public void PrintHeader(Item item, IReadOnlyDictionary<int, UpgradeComponent> upgrades)
    {
        FlowPanel header = new()
        {
            Parent = _layout,
            FlowDirection = ControlFlowDirection.SingleLeftToRight,
            ControlPadding = new Vector2(5f),
            Width = _layout.Width,
            Height = 50
        };

        ItemImage icon = new(item)
        {
            Parent = header
        };

        ItemName name = new(item, upgrades)
        {
            Parent = header,
            Width = _layout.Width - 55,
            Height = 50,
            VerticalAlignment = VerticalAlignment.Middle,
            Font = GameService.Content.DefaultFont18,
            WrapText = true,
            SuffixItem = item.SuffixItem(upgrades) ?? item.SecondarySuffixItem(upgrades)
        };

        name.Text = name.Text.Replace(" ", "  ");
    }

    public void PrintDefense(int defense)
    {
        if (defense > 0)
        {
            PrintPlainText($"Defense: {defense:N0}");
        }
    }

    public void PrintAttributes(IReadOnlyDictionary<string, int> attributes)
    {
        if (attributes.Count > 0)
        {
            StringBuilder builder = new();
            foreach (KeyValuePair<string, int> stat in attributes)
            {
                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.AppendFormat($"+{stat.Value:N0} {stat.Key}");
            }

            PrintPlainText(builder.ToString());
        }
    }

    public void PrintUpgrades(IUpgradable item, ItemFlags flags, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades)
    {
        ItemUpgradeBuilder upgradeBuilder = new(flags, icons, upgrades);
        upgradeBuilder.AddSuffixItem(item.SuffixItemId);
        upgradeBuilder.AddSecondarySuffixItemId(item.SecondarySuffixItemId);
        upgradeBuilder.AddInfusionSlots(item.InfusionSlots);
        if (item is Weapon { TwoHanded: true })
        {
            upgradeBuilder.TwoHanded();
        }

        upgradeBuilder.Build(_layout);
    }

    public void PrintItemSkin(int skinId)
    {
        // TODO: unlock status
    }

    public void PrintItemRarity(Extensible<Rarity> rarity)
    {
        if (rarity != Rarity.Basic)
        {
            PrintPlainText($"\r\n{rarity}", ItemColors.Rarity(rarity));
        }
    }

    public void PrintWeightClass(Extensible<WeightClass> weightClass)
    {
        PrintPlainText(weightClass.ToString());
    }

    public void PrintRequiredLevel(int level)
    {
        if (level > 0)
        {
            PrintPlainText($"Required Level: {level}");
        }
    }

    public void PrintDescription(string description, bool finalNewLine = false)
    {
        if (string.IsNullOrEmpty(description))
        {
            return;
        }

        Panel container = new() { Parent = _layout, Width = _layout.Width, HeightSizingMode = SizingMode.AutoSize };

        FormattedLabelBuilder builder = new FormattedLabelBuilder()
            .SetWidth(_layout.Width - 10)
            .AutoSizeHeight()
            .Wrap()
            .AddMarkup(description);
        if (finalNewLine)
        {
            builder.CreatePart("\r\n\r\n", part => part.SetFontSize(ContentService.FontSize.Size16));
        }

        FormattedLabel label = builder.Build();
        label.Parent = container;
    }

    public void PrintInBank()
    {
        // TODO: bank count
    }

    public void PrintItemBinding(Item item)
    {
        if (item is Currency or Service)
        {
            return;
        }

        if (item.Flags.AccountBound)
        {
            PrintPlainText("Account Bound on Acquire");
        }
        else if (item.Flags.AccountBindOnUse)
        {
            PrintPlainText("Account Bound on Use");
        }

        if (item.Flags.Soulbound)
        {
            PrintPlainText("Soulbound on Acquire");
        }
        else if (item.Flags.SoulbindOnUse)
        {
            PrintPlainText("Soulbound on Use");
        }
    }

    public void PrintVendorValue(Item item)
    {
        if (item.VendorValue == Coin.Zero || item.Flags.NoSell)
        {
            return;
        }

        FormattedLabelBuilder? builder = new FormattedLabelBuilder()
            .SetWidth(_layout.Width)
            .AutoSizeHeight();

        if (item.VendorValue.Amount >= 10_000)
        {
            FormattedLabelPartBuilder? gold = builder.CreatePart(item.VendorValue.Gold.ToString("N0"));
            gold.SetTextColor(new Color(0xDD, 0xBB, 0x44));
            gold.SetFontSize(ContentService.FontSize.Size16);
            gold.SetSuffixImage(AsyncTexture2D.FromAssetId(156904));
            gold.SetSuffixImageSize(new Point(20));
            builder.CreatePart(gold);
            builder.CreatePart("  ", _ => { });
        }

        if (item.VendorValue.Amount >= 100)
        {
            FormattedLabelPartBuilder? silver = builder.CreatePart(item.VendorValue.Silver.ToString("N0"));
            silver.SetTextColor(new Color(0xC0, 0xC0, 0xC0));
            silver.SetFontSize(ContentService.FontSize.Size16);
            silver.SetSuffixImage(AsyncTexture2D.FromAssetId(156907));
            silver.SetSuffixImageSize(new Point(20));
            builder.CreatePart(silver);
            builder.CreatePart("  ", _ => { });
        }

        FormattedLabelPartBuilder? copper = builder.CreatePart(item.VendorValue.Copper.ToString("N0"));
        copper.SetTextColor(new Color(0xCD, 0x7F, 0x32));
        copper.SetFontSize(ContentService.FontSize.Size16);
        copper.SetSuffixImage(AsyncTexture2D.FromAssetId(156902));
        copper.SetSuffixImageSize(new Point(20));
        builder.CreatePart(copper);

        FormattedLabel? label = builder.Build();
        label.Parent = _layout;
        label.Width = _layout.Width;
    }

    public void PrintEffect(Effect effect)
    {
        FlowPanel panel = new()
        {
            Parent = _layout,
            FlowDirection = ControlFlowDirection.SingleLeftToRight,
            Width = _layout.Width,
            HeightSizingMode = SizingMode.AutoSize,
            ControlPadding = new Vector2(5f)
        };

        if (!string.IsNullOrEmpty(effect.IconHref))
        {
            Image icon = new()
            {
                Parent = panel,
                Texture = GameService.Content.GetRenderServiceTexture(effect.IconHref),
                Size = new Point(32)
            };
        }

        StringBuilder builder = new();
        builder.Append(effect.Name);

        if (effect.Duration > TimeSpan.Zero)
        {
            builder.AppendFormat(" ({0})", effect.Duration switch
            {
                { Hours: >= 1 } => $"{effect.Duration.TotalHours} h",
                _ => $"{effect.Duration.TotalMinutes} m"
            });
        }

        builder.Append(": ");
        builder.Append(effect.Description);

        Label label = new()
        {
            Parent = panel,
            Width = panel.Width - 30,
            AutoSizeHeight = true,
            WrapText = true,
            Text = builder.ToString(),
            TextColor = new Color(0xAA, 0xAA, 0xAA),
            Font = GameService.Content.DefaultFont16
        };
    }

    public void PrintMini(int miniatureId)
    {
        // TODO: mini unlock status
    }

    public void PrintBuff(Buff buff)
    {
        FormattedLabel? label = new FormattedLabelBuilder()
            .SetWidth(_layout.Width)
            .AutoSizeHeight()
            .Wrap()
            .CreatePart("\r\n", part => part.SetFontSize(ContentService.FontSize.Size16))
            .AddMarkup(buff.Description, new Color(0x55, 0x99, 0xFF))
            .Build();
        label.Parent = _layout;
    }

    public void PrintBonuses(IReadOnlyList<string> bonuses)
    {
        StringBuilder text = new();
        foreach ((string? bonus, int ordinal) in bonuses
            .Select((value, index) => (value, index + 1)))
        {
            text.Append($"\r\n({ordinal:0}): {bonus}");
        }

        PrintPlainText(text.ToString(), new Color(0x99, 0x99, 0x99));
    }

    public void PrintWeaponStrength(Weapon weapon)
    {
        FormattedLabelBuilder? builder = new FormattedLabelBuilder()
            .SetWidth(_layout.Width)
            .AutoSizeHeight()
            .Wrap();

        builder.CreatePart($"Weapon Strength: {weapon.MinPower:N0} - {weapon.MaxPower:N0}", static part =>
        {
            part.SetFontSize(ContentService.FontSize.Size16);
        });

        if (weapon.DamageType.IsDefined())
        {
            switch (weapon.DamageType.ToEnum())
            {
                case DamageType.Choking:
                    builder.CreatePart(" (Choking)", static part =>
                    {
                        part.SetFontSize(ContentService.FontSize.Size16);
                        part.SetTextColor(new Color(0x99, 0x99, 0x99));
                    });
                    break;
                case DamageType.Fire:
                    builder.CreatePart(" (Fire)", static part =>
                    {
                        part.SetFontSize(ContentService.FontSize.Size16);
                        part.SetTextColor(new Color(0x99, 0x99, 0x99));
                    });
                    break;
                case DamageType.Ice:
                    builder.CreatePart(" (Ice)", static part =>
                    {
                        part.SetFontSize(ContentService.FontSize.Size16);
                        part.SetTextColor(new Color(0x99, 0x99, 0x99));
                    });
                    break;
                case DamageType.Lightning:
                    builder.CreatePart(" (Lightning)", static part =>
                    {
                        part.SetFontSize(ContentService.FontSize.Size16);
                        part.SetTextColor(new Color(0x99, 0x99, 0x99));
                    });
                    break;
            }
        }

        FormattedLabel? label = builder.Build();
        label.Parent = _layout;
    }

    public void PrintStatChoices(ICombatEquipment equipment)
    {
        if (equipment.StatChoices.Count > 0)
        {
            PrintPlainText("Double-click to select stats.");
        }
    }

    public void PrintUniqueness(Item item)
    {
        if (item.Flags.Unique)
        {
            PrintPlainText("Unique");
        }
    }

    protected override void Build(Container buildPanel)
    {
        _layout.Parent = buildPanel;
    }
}