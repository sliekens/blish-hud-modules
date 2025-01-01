using System.Text;

using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.Common.Controls.Items.Upgrades;

using Color = Microsoft.Xna.Framework.Color;
using Container = Blish_HUD.Controls.Container;
using Currency = GuildWars2.Items.Currency;
using Item = GuildWars2.Items.Item;

namespace SL.Common.Controls.Items;

public class ItemTooltipView(Item item, IReadOnlyDictionary<int, UpgradeComponent> upgrades) : View, IItemTooltipView
{
    private readonly ItemIcons _icons = ServiceLocator.GetService<ItemIcons>();

    protected override void Build(Container buildPanel)
    {
        FlowPanel layout = new()
        {
            FlowDirection = ControlFlowDirection.SingleTopToBottom,
            Width = 350,
            HeightSizingMode = SizingMode.AutoSize,
            Parent = buildPanel
        };

        switch (item)
        {
            case Armor armor:
                PrintArmor(armor, _icons, upgrades, layout);
                break;
            case Backpack back:
                PrintBackpack(back, _icons, upgrades, layout);
                break;
            case Bag bag:
                PrintBag(bag, _icons, upgrades, layout);
                break;
            case Consumable consumable:
                PrintConsumable(consumable, _icons, upgrades, layout);
                break;
            case GuildWars2.Items.Container container:
                PrintContainer(container, _icons, upgrades, layout);
                break;
            case CraftingMaterial craftingMaterial:
                PrintCraftingMaterial(craftingMaterial, _icons, upgrades, layout);
                break;
            case GatheringTool gatheringTool:
                PrintGatheringTool(gatheringTool, _icons, upgrades, layout);
                break;
            case Trinket trinket:
                PrintTrinket(trinket, _icons, upgrades, layout);
                break;
            case Gizmo gizmo:
                PrintGizmo(gizmo, _icons, upgrades, layout);
                break;
            case JadeTechModule jadeTechModule:
                PrintJadeTechModule(jadeTechModule, _icons, upgrades, layout);
                break;
            case Miniature miniature:
                PrintMiniature(miniature, _icons, upgrades, layout);
                break;
            case PowerCore powerCore:
                PrintPowerCore(powerCore, _icons, upgrades, layout);
                break;
            case Relic relic:
                PrintRelic(relic, _icons, upgrades, layout);
                break;
            case SalvageTool salvageTool:
                PrintSalvageTool(salvageTool, _icons, upgrades, layout);
                break;
            case Trophy trophy:
                PrintTrophy(trophy, _icons, upgrades, layout);
                break;
            case UpgradeComponent upgradeComponent:
                PrintUpgradeComponent(upgradeComponent, _icons, upgrades, layout);
                break;
            case Weapon weapon:
                PrintWeapon(weapon, _icons, upgrades, layout);
                break;
            default:
                Print(item, _icons, upgrades, layout);
                break;
        }

        static void PrintArmor(Armor item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);
            Defense(item.Defense, layout);
            Attributes(item.Attributes, layout);

            ItemUpgradeBuilder upgradeBuilder = new(item.Flags, icons, upgrades);
            upgradeBuilder.AddSuffixItem(item.SuffixItemId);
            upgradeBuilder.AddInfusionSlots(item.InfusionSlots);
            upgradeBuilder.Build(layout);

            ItemSkin(item.DefaultSkinId, layout);
            ItemRarity(item, layout);
            PlainText(item.WeightClass.ToString(), layout);
            switch (item)
            {
                case Boots:
                    PlainText("Foot Armor", layout);
                    break;
                case Coat:
                    PlainText("Chest Armor", layout);
                    break;
                case Gloves:
                    PlainText("Hand Armor", layout);
                    break;
                case Helm:
                case HelmAquatic:
                    PlainText("Head Armor", layout);
                    break;
                case Leggings:
                    PlainText("Leg Armor", layout);
                    break;
                case Shoulders:
                    PlainText("Shoulder Armor", layout);
                    break;
            }

            RequiredLevel(item, layout);
            Description(item, layout);
            InBank(item, layout);
            if (item.StatChoices.Count > 0)
            {
                PlainText("Double-click to select stats.", layout);
            }

            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void PrintBackpack(Backpack item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);

            Attributes(item.Attributes, layout);

            ItemUpgradeBuilder upgradeBuilder = new(item.Flags, icons, upgrades);
            upgradeBuilder.AddSuffixItem(item.SuffixItemId);
            upgradeBuilder.AddInfusionSlots(item.InfusionSlots);
            upgradeBuilder.Build(layout);

            ItemSkin(item.DefaultSkinId, layout);

            ItemRarity(item, layout);
            PlainText("Back Item", layout);
            RequiredLevel(item, layout);
            Description(item, layout);

            InBank(item, layout);
            if (item.StatChoices.Count > 0)
            {
                PlainText("Double-click to select stats.", layout);
            }

            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void PrintBag(Bag item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);
            Description(item, layout);

            InBank(item, layout);
            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void PrintConsumable(Consumable item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);
            switch (item)
            {
                case Currency or Service:
                    PlainText("Takes effect immediately upon receipt.", layout);
                    break;
                default:
                    PlainText("Double-click to consume.", layout);
                    break;
            }

            switch (item)
            {
                case Food { Effect: not null } food:
                    Effect(food.Effect, layout);
                    break;
                case Utility { Effect: not null } utility:
                    Effect(utility.Effect, layout);
                    break;
                case Service { Effect: not null } service:
                    Effect(service.Effect, layout);
                    break;
                case GenericConsumable { Effect: not null } generic:
                    Effect(generic.Effect, layout);
                    break;
            }

            Description(item, layout);

            switch (item)
            {
                case Currency or Service:
                    PlainText(string.IsNullOrEmpty(item.Description) ? "Service" : "\r\nService", layout);
                    break;
                case Transmutation transmutation:
                    ItemSkin(transmutation.SkinIds.First(), layout);
                    PlainText("\r\nConsumable", layout);
                    break;
                case Booze:
                    PlainText("""

                              Excessive alcohol consumption will result in intoxication.

                              Consumable          
                              """, layout);
                    break;
                default:
                    PlainText(string.IsNullOrEmpty(item.Description) ? "Consumable" : "\r\nConsumable", layout);
                    break;
            }

            RequiredLevel(item, layout);
            InBank(item, layout);
            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void PrintContainer(GuildWars2.Items.Container item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);
            Description(item, layout);
            PlainText(string.IsNullOrEmpty(item.Description) ? "Consumable" : "\r\nConsumable", layout);
            InBank(item, layout);
            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void PrintCraftingMaterial(CraftingMaterial item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);
            Description(item, layout);
            InBank(item, layout);
            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void PrintGatheringTool(GatheringTool item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);
            Description(item, layout);

            InBank(item, layout);
            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void PrintGizmo(Gizmo item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);
            Description(item, layout, item.Level > 0);

            RequiredLevel(item, layout);

            InBank(item, layout);
            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void PrintJadeTechModule(JadeTechModule item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);
            Description(item, layout, true);

            ItemRarity(item, layout);
            PlainText("Module", layout);
            RequiredLevel(item, layout);
            PlainText("Required Mastery: Jade Bots", layout);

            InBank(item, layout);
            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void PrintMiniature(Miniature item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);
            Description(item, layout);

            Mini(item.MiniatureId, layout);

            PlainText("Mini", layout);

            InBank(item, layout);
            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void PrintPowerCore(PowerCore item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);
            Description(item, layout, true);

            ItemRarity(item, layout);
            PlainText("Power Core", layout);
            RequiredLevel(item, layout);

            InBank(item, layout);
            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void PrintRelic(Relic item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);
            Description(item, layout, true);
            ItemRarity(item, layout);
            PlainText("Relic", layout);
            RequiredLevel(item, layout);

            InBank(item, layout);
            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void PrintSalvageTool(SalvageTool item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);
            PlainText(" ", layout);
            ItemRarity(item, layout);
            PlainText("Consumable", layout);
            Description(item, layout);

            InBank(item, layout);
            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void PrintTrinket(Trinket item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);

            Attributes(item.Attributes, layout);

            ItemUpgradeBuilder upgradeBuilder = new(item.Flags, icons, upgrades);
            upgradeBuilder.AddSuffixItem(item.SuffixItemId);
            upgradeBuilder.AddInfusionSlots(item.InfusionSlots);
            upgradeBuilder.Build(layout);

            ItemRarity(item, layout);

            switch (item)
            {
                case Accessory:
                    PlainText("Accessory", layout);
                    break;
                case Amulet:
                    PlainText("Amulet", layout);
                    break;
                case Ring:
                    PlainText("Ring", layout);
                    break;
            }

            RequiredLevel(item, layout);
            Description(item, layout);

            InBank(item, layout);
            if (item.StatChoices.Count > 0)
            {
                PlainText("Double-click to select stats.", layout);
            }

            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void PrintTrophy(Trophy item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);
            Description(item, layout, true);
            PlainText("Trophy", layout);

            InBank(item, layout);
            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void PrintUpgradeComponent(UpgradeComponent item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);

            if (item is Rune rune)
            {
                StringBuilder bonuses = new();
                foreach ((string? bonus, int ordinal) in (rune.Bonuses ?? []).Select((value, index) =>
                             (value, index + 1)))
                {
                    bonuses.Append($"\r\n({ordinal:0}): {bonus}");
                }

                PlainText(bonuses.ToString(), layout, new Color(0x99, 0x99, 0x99));
            }
            else if (item.Buff is { Description.Length: > 0 })
            {
                FormattedLabel? label = new FormattedLabelBuilder()
                    .SetWidth(layout.Width)
                    .AutoSizeHeight()
                    .Wrap()
                    .CreatePart("\r\n", part => part.SetFontSize(ContentService.FontSize.Size16))
                    .AddMarkup(item.Buff!.Description, new Color(0x55, 0x99, 0xFF))
                    .Build();
                label.Parent = layout;
            }
            else
            {
                StringBuilder builder = new();
                foreach (KeyValuePair<Extensible<AttributeName>, int> stat in item.Attributes)
                {
                    builder.Append($"+{stat.Value:N0} {AttributeName(stat.Key)}");
                }

                PlainText(builder.ToString(), layout, new Color(0x55, 0x99, 0xFF));
            }


            Description(item, layout);

            RequiredLevel(item, layout);

            InBank(item, layout);
            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void PrintWeapon(Weapon item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);
            WeaponStrength(item, layout);
            Defense(item.Defense, layout);
            Attributes(item.Attributes, layout);

            ItemUpgradeBuilder upgradeBuilder = new(item.Flags, icons, upgrades);
            upgradeBuilder.AddSuffixItem(item.SuffixItemId);
            upgradeBuilder.AddSecondarySuffixItemId(item.SecondarySuffixItemId);
            upgradeBuilder.AddInfusionSlots(item.InfusionSlots);
            if (item is Greatsword or Hammer or Longbow or Rifle or Shortbow or Staff or Spear or HarpoonGun or Trident)
            {
                upgradeBuilder.TwoHanded();
            }

            upgradeBuilder.Build(layout);

            ItemSkin(item.DefaultSkinId, layout);
            ItemRarity(item, layout);
            switch (item)
            {
                case Axe:
                    PlainText("Axe", layout);
                    break;
                case Dagger:
                    PlainText("Dagger", layout);
                    break;
                case Focus:
                    PlainText("Focus", layout);
                    break;
                case Greatsword:
                    PlainText("Greatsword", layout);
                    break;
                case Hammer:
                    PlainText("Hammer", layout);
                    break;
                case HarpoonGun:
                    PlainText("Harpoon Gun", layout);
                    break;
                case LargeBundle:
                    PlainText("Large Bundle", layout);
                    break;
                case Longbow:
                    PlainText("Longbow", layout);
                    break;
                case Mace:
                    PlainText("Mace", layout);
                    break;
                case Pistol:
                    PlainText("Pistol", layout);
                    break;
                case Rifle:
                    PlainText("Rifle", layout);
                    break;
                case Scepter:
                    PlainText("Scepter", layout);
                    break;
                case Shield:
                    PlainText("Shield", layout);
                    break;
                case Shortbow:
                    PlainText("Shortbow", layout);
                    break;
                case SmallBundle:
                    PlainText("Small Bundle", layout);
                    break;
                case Spear:
                    PlainText("Spear", layout);
                    break;
                case Staff:
                    PlainText("Staff", layout);
                    break;
                case Sword:
                    PlainText("Sword", layout);
                    break;
                case Torch:
                    PlainText("Torch", layout);
                    break;
                case Toy:
                    PlainText("Toy", layout);
                    break;
                case ToyTwoHanded:
                    PlainText("Toy", layout);
                    break;
                case Trident:
                    PlainText("Trident", layout);
                    break;
                case Warhorn:
                    PlainText("Warhorn", layout);
                    break;
            }

            RequiredLevel(item, layout);

            Description(item, layout);

            InBank(item, layout);
            if (item.StatChoices.Count > 0)
            {
                PlainText("Double-click to select stats.", layout);
            }

            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void Print(Item item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container layout)
        {
            Header(item, icons, upgrades, layout);
            Description(item, layout);

            InBank(item, layout);
            if (item.Flags.Unique)
            {
                PlainText("Unique", layout);
            }

            AccountBound(item, layout);
            SoulBound(item, layout);
            VendorValue(item, layout);
        }

        static void Header(Item item, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades, Container parent)
        {
            FlowPanel header = new()
            {
                Parent = parent,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5f),
                Width = parent.Width,
                Height = 50
            };

            ItemImage icon = new(item)
            {
                Parent = header
            };

            ItemName name = new(item, upgrades)
            {
                Parent = header,
                Width = parent.Width - 55,
                Height = 50,
                VerticalAlignment = VerticalAlignment.Middle,
                Font = GameService.Content.DefaultFont18,
                WrapText = true,
                SuffixItem = item.SuffixItem(upgrades) ?? item.SecondarySuffixItem(upgrades)
            };

            name.Text = name.Text.Replace(" ", "  ");
        }

        static Control? WeaponStrength(Weapon item, Container parent)
        {
            FormattedLabelBuilder? builder = new FormattedLabelBuilder()
                .SetWidth(parent.Width)
                .AutoSizeHeight()
                .Wrap();

            builder.CreatePart($"Weapon Strength: {item.MinPower:N0} - {item.MaxPower:N0}", static part =>
            {
                part.SetFontSize(ContentService.FontSize.Size16);
            });

            if (item.DamageType.IsDefined())
            {
                switch (item.DamageType.ToEnum())
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
            label.Parent = parent;
            return label;
        }

        static Control? Defense(int defense, Container parent)
        {
            if (defense == 0)
            {
                return null;
            }

            return PlainText($"Defense: {defense:N0}", parent);
        }

        static Control? Attributes(IDictionary<Extensible<AttributeName>, int> attributes, Container parent)
        {
            if (attributes.Count == 0)
            {
                return null;
            }

            StringBuilder builder = new();
            foreach (KeyValuePair<Extensible<AttributeName>, int> stat in attributes)
            {
                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.AppendFormat($"+{stat.Value:N0} {AttributeName(stat.Key)}");
            }

            return PlainText(builder.ToString(), parent);
        }

        static string AttributeName(Extensible<AttributeName> stat)
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

        static Control Effect(Effect effect, Container parent)
        {
            FlowPanel panel = new()
            {
                Parent = parent,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                Width = parent.Width,
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

            return panel;
        }

        static Control? Description(Item item, Container parent, bool finalNewLine = false)
        {
            if (string.IsNullOrEmpty(item.Description))
            {
                return null;
            }

            Panel container = new() { Parent = parent, Width = parent.Width, HeightSizingMode = SizingMode.AutoSize };

            FormattedLabelBuilder builder = new FormattedLabelBuilder()
                .SetWidth(parent.Width - 10)
                .AutoSizeHeight()
                .Wrap()
                .AddMarkup(item.Description);
            if (finalNewLine)
            {
                builder.CreatePart("\r\n\r\n", part => part.SetFontSize(ContentService.FontSize.Size16));
            }

            FormattedLabel label = builder.Build();
            label.Parent = container;
            return label;
        }

        static Control? Mini(int miniatureId, Container parent)
        {
            // TODO: mini unlock status
            return null;
        }

        static Control? ItemSkin(int skinId, Container parent)
        {
            // TODO: skin unlock status
            return null;
        }

        static Control? ItemRarity(Item item, Container parent)
        {
            if (item.Rarity == Rarity.Basic)
            {
                return null;
            }

            return PlainText($"\r\n{item.Rarity}", parent, ItemColors.Rarity(item.Rarity));
        }

        static Control? RequiredLevel(Item item, Container parent)
        {
            if (item.Level == 0)
            {
                return null;
            }

            return PlainText($"Required Level: {item.Level}", parent);
        }

        static Control? InBank(Item item, Container parent)
        {
            return null;
        }

        static Control? AccountBound(Item item, Container parent)
        {
            if (item is Currency or Service)
            {
                return null;
            }

            if (item.Flags.AccountBound)
            {
                return PlainText("Account Bound on Acquire", parent);
            }

            if (item.Flags.AccountBindOnUse)
            {
                return PlainText("Account Bound on Use", parent);
            }

            return null;
        }

        static Control? SoulBound(Item item, Container parent)
        {
            if (item is Currency or Service)
            {
                return null;
            }

            if (item.Flags.Soulbound)
            {
                return PlainText("Soulbound on Acquire", parent);
            }

            if (item.Flags.SoulbindOnUse)
            {
                return PlainText("Soulbound on Use", parent);
            }

            return null;
        }

        static Control? VendorValue(Item item, Container parent)
        {
            Coin vendorValue = item.VendorValue;
            if (vendorValue == Coin.Zero || item.Flags.NoSell)
            {
                return null;
            }

            FormattedLabelBuilder? builder = new FormattedLabelBuilder()
                .SetWidth(parent.Width)
                .AutoSizeHeight();

            if (vendorValue.Amount >= 10_000)
            {
                FormattedLabelPartBuilder? gold = builder.CreatePart(vendorValue.Gold.ToString("N0"));
                gold.SetTextColor(new Color(0xDD, 0xBB, 0x44));
                gold.SetFontSize(ContentService.FontSize.Size16);
                gold.SetSuffixImage(AsyncTexture2D.FromAssetId(156904));
                gold.SetSuffixImageSize(new Point(20));
                builder.CreatePart(gold);
                builder.CreatePart("  ", _ => { });
            }

            if (vendorValue.Amount >= 100)
            {
                FormattedLabelPartBuilder? silver = builder.CreatePart(vendorValue.Silver.ToString("N0"));
                silver.SetTextColor(new Color(0xC0, 0xC0, 0xC0));
                silver.SetFontSize(ContentService.FontSize.Size16);
                silver.SetSuffixImage(AsyncTexture2D.FromAssetId(156907));
                silver.SetSuffixImageSize(new Point(20));
                builder.CreatePart(silver);
                builder.CreatePart("  ", _ => { });
            }

            FormattedLabelPartBuilder? copper = builder.CreatePart(vendorValue.Copper.ToString("N0"));
            copper.SetTextColor(new Color(0xCD, 0x7F, 0x32));
            copper.SetFontSize(ContentService.FontSize.Size16);
            copper.SetSuffixImage(AsyncTexture2D.FromAssetId(156902));
            copper.SetSuffixImageSize(new Point(20));
            builder.CreatePart(copper);

            FormattedLabel? label = builder.Build();
            label.Parent = parent;
            label.Width = parent.Width;

            return label;
        }

        static Label PlainText(string text, Container parent, Color? textColor = null)
        {
            return new Label
            {
                Parent = parent,
                Width = parent.Width,
                AutoSizeHeight = true,
                Text = text,
                TextColor = textColor.GetValueOrDefault(Color.White),
                Font = GameService.Content.DefaultFont16,
                WrapText = true
            };
        }
    }
}