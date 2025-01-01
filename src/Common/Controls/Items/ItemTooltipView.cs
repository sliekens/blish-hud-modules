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
                PrintArmor(armor);
                break;
            case Backpack back:
                PrintBackpack(back);
                break;
            case Bag bag:
                PrintBag(bag);
                break;
            case Consumable consumable:
                PrintConsumable(consumable);
                break;
            case GuildWars2.Items.Container container:
                PrintContainer(container);
                break;
            case CraftingMaterial craftingMaterial:
                PrintCraftingMaterial(craftingMaterial);
                break;
            case GatheringTool gatheringTool:
                PrintGatheringTool(gatheringTool);
                break;
            case Trinket trinket:
                PrintTrinket(trinket);
                break;
            case Gizmo gizmo:
                PrintGizmo(gizmo);
                break;
            case JadeTechModule jadeTechModule:
                PrintJadeTechModule(jadeTechModule);
                break;
            case Miniature miniature:
                PrintMiniature(miniature);
                break;
            case PowerCore powerCore:
                PrintPowerCore(powerCore);
                break;
            case Relic relic:
                PrintRelic(relic);
                break;
            case SalvageTool salvageTool:
                PrintSalvageTool(salvageTool);
                break;
            case Trophy trophy:
                PrintTrophy(trophy);
                break;
            case UpgradeComponent upgradeComponent:
                PrintUpgradeComponent(upgradeComponent);
                break;
            case Weapon weapon:
                PrintWeapon(weapon);
                break;
            default:
                Print();
                break;
        }

        void PrintArmor(Armor armor)
        {
            Header();
            Defense(armor.Defense);
            Attributes(armor.Attributes);

            ItemUpgradeBuilder upgradeBuilder = new(armor.Flags, _icons, upgrades);
            upgradeBuilder.AddSuffixItem(armor.SuffixItemId);
            upgradeBuilder.AddInfusionSlots(armor.InfusionSlots);
            upgradeBuilder.Build(layout);

            ItemSkin(armor.DefaultSkinId);
            ItemRarity();
            PlainText(armor.WeightClass.ToString());
            switch (armor)
            {
                case Boots:
                    PlainText("Foot Armor");
                    break;
                case Coat:
                    PlainText("Chest Armor");
                    break;
                case Gloves:
                    PlainText("Hand Armor");
                    break;
                case Helm:
                case HelmAquatic:
                    PlainText("Head Armor");
                    break;
                case Leggings:
                    PlainText("Leg Armor");
                    break;
                case Shoulders:
                    PlainText("Shoulder Armor");
                    break;
            }

            RequiredLevel();
            Description();
            InBank();
            if (armor.StatChoices.Count > 0)
            {
                PlainText("Double-click to select stats.");
            }

            if (armor.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void PrintBackpack(Backpack back)
        {
            Header();

            Attributes(back.Attributes);

            ItemUpgradeBuilder upgradeBuilder = new(back.Flags, _icons, upgrades);
            upgradeBuilder.AddSuffixItem(back.SuffixItemId);
            upgradeBuilder.AddInfusionSlots(back.InfusionSlots);
            upgradeBuilder.Build(layout);

            ItemSkin(back.DefaultSkinId);

            ItemRarity();
            PlainText("Back Item");
            RequiredLevel();
            Description();

            InBank();
            if (back.StatChoices.Count > 0)
            {
                PlainText("Double-click to select stats.");
            }

            if (back.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void PrintBag(Bag bag)
        {
            Header();
            Description();

            InBank();
            if (bag.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void PrintConsumable(Consumable consumable)
        {
            Header();
            switch (consumable)
            {
                case Currency or Service:
                    PlainText("Takes effect immediately upon receipt.");
                    break;
                default:
                    PlainText("Double-click to consume.");
                    break;
            }

            switch (consumable)
            {
                case Food { Effect: not null } food:
                    Effect(food.Effect);
                    break;
                case Utility { Effect: not null } utility:
                    Effect(utility.Effect);
                    break;
                case Service { Effect: not null } service:
                    Effect(service.Effect);
                    break;
                case GenericConsumable { Effect: not null } generic:
                    Effect(generic.Effect);
                    break;
            }

            Description();

            switch (consumable)
            {
                case Currency or Service:
                    PlainText(string.IsNullOrEmpty(consumable.Description) ? "Service" : "\r\nService");
                    break;
                case Transmutation transmutation:
                    ItemSkin(transmutation.SkinIds.First());
                    PlainText("\r\nConsumable");
                    break;
                case Booze:
                    PlainText("""

                              Excessive alcohol consumption will result in intoxication.

                              Consumable          
                              """);
                    break;
                default:
                    PlainText(string.IsNullOrEmpty(consumable.Description) ? "Consumable" : "\r\nConsumable");
                    break;
            }

            RequiredLevel();
            InBank();
            if (consumable.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void PrintContainer(GuildWars2.Items.Container container)
        {
            Header();
            Description();
            PlainText(string.IsNullOrEmpty(container.Description) ? "Consumable" : "\r\nConsumable");
            InBank();
            if (container.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void PrintCraftingMaterial(CraftingMaterial craftingMaterial)
        {
            Header();
            Description();
            InBank();
            if (craftingMaterial.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void PrintGatheringTool(GatheringTool gatheringTool)
        {
            Header();
            Description();

            InBank();
            if (gatheringTool.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void PrintGizmo(Gizmo gizmo)
        {
            Header();
            Description(gizmo.Level > 0);

            RequiredLevel();

            InBank();
            if (gizmo.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void PrintJadeTechModule(JadeTechModule jadeTechModule)
        {
            Header();
            Description(true);

            ItemRarity();
            PlainText("Module");
            RequiredLevel();
            PlainText("Required Mastery: Jade Bots");

            InBank();
            if (jadeTechModule.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void PrintMiniature(Miniature miniature)
        {
            Header();
            Description();

            Mini(miniature.MiniatureId);

            PlainText("Mini");

            InBank();
            if (miniature.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void PrintPowerCore(PowerCore powerCore)
        {
            Header();
            Description(true);

            ItemRarity();
            PlainText("Power Core");
            RequiredLevel();

            InBank();
            if (powerCore.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void PrintRelic(Relic relic)
        {
            Header();
            Description(true);
            ItemRarity();
            PlainText("Relic");
            RequiredLevel();

            InBank();
            if (relic.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void PrintSalvageTool(SalvageTool salvageTool)
        {
            Header();
            PlainText(" ");
            ItemRarity();
            PlainText("Consumable");
            Description();

            InBank();
            if (salvageTool.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void PrintTrinket(Trinket trinket)
        {
            Header();

            Attributes(trinket.Attributes);

            ItemUpgradeBuilder upgradeBuilder = new(trinket.Flags, _icons, upgrades);
            upgradeBuilder.AddSuffixItem(trinket.SuffixItemId);
            upgradeBuilder.AddInfusionSlots(trinket.InfusionSlots);
            upgradeBuilder.Build(layout);

            ItemRarity();

            switch (trinket)
            {
                case Accessory:
                    PlainText("Accessory");
                    break;
                case Amulet:
                    PlainText("Amulet");
                    break;
                case Ring:
                    PlainText("Ring");
                    break;
            }

            RequiredLevel();
            Description();

            InBank();
            if (trinket.StatChoices.Count > 0)
            {
                PlainText("Double-click to select stats.");
            }

            if (trinket.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void PrintTrophy(Trophy trophy)
        {
            Header();
            Description(true);
            PlainText("Trophy");

            InBank();
            if (trophy.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void PrintUpgradeComponent(UpgradeComponent upgradeComponent)
        {
            Header();

            if (upgradeComponent is Rune rune)
            {
                StringBuilder bonuses = new();
                foreach ((string? bonus, int ordinal) in (rune.Bonuses ?? []).Select((value, index) =>
                             (value, index + 1)))
                {
                    bonuses.Append($"\r\n({ordinal:0}): {bonus}");
                }

                PlainText(bonuses.ToString(), new Color(0x99, 0x99, 0x99));
            }
            else if (upgradeComponent.Buff is { Description.Length: > 0 })
            {
                FormattedLabel? label = new FormattedLabelBuilder()
                    .SetWidth(layout.Width)
                    .AutoSizeHeight()
                    .Wrap()
                    .CreatePart("\r\n", part => part.SetFontSize(ContentService.FontSize.Size16))
                    .AddMarkup(upgradeComponent.Buff!.Description, new Color(0x55, 0x99, 0xFF))
                    .Build();
                label.Parent = layout;
            }
            else
            {
                StringBuilder builder = new();
                foreach (KeyValuePair<Extensible<AttributeName>, int> stat in upgradeComponent.Attributes)
                {
                    builder.Append($"+{stat.Value:N0} {AttributeName(stat.Key)}");
                }

                PlainText(builder.ToString(), new Color(0x55, 0x99, 0xFF));
            }


            Description();

            RequiredLevel();

            InBank();
            if (upgradeComponent.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void PrintWeapon(Weapon weapon)
        {
            Header();
            WeaponStrength(weapon);
            Defense(weapon.Defense);
            Attributes(weapon.Attributes);

            ItemUpgradeBuilder upgradeBuilder = new(weapon.Flags, _icons, upgrades);
            upgradeBuilder.AddSuffixItem(weapon.SuffixItemId);
            upgradeBuilder.AddSecondarySuffixItemId(weapon.SecondarySuffixItemId);
            upgradeBuilder.AddInfusionSlots(weapon.InfusionSlots);
            if (weapon is Greatsword or Hammer or Longbow or Rifle or Shortbow or Staff or Spear or HarpoonGun or Trident)
            {
                upgradeBuilder.TwoHanded();
            }

            upgradeBuilder.Build(layout);

            ItemSkin(weapon.DefaultSkinId);
            ItemRarity();
            switch (weapon)
            {
                case Axe:
                    PlainText("Axe");
                    break;
                case Dagger:
                    PlainText("Dagger");
                    break;
                case Focus:
                    PlainText("Focus");
                    break;
                case Greatsword:
                    PlainText("Greatsword");
                    break;
                case Hammer:
                    PlainText("Hammer");
                    break;
                case HarpoonGun:
                    PlainText("Harpoon Gun");
                    break;
                case LargeBundle:
                    PlainText("Large Bundle");
                    break;
                case Longbow:
                    PlainText("Longbow");
                    break;
                case Mace:
                    PlainText("Mace");
                    break;
                case Pistol:
                    PlainText("Pistol");
                    break;
                case Rifle:
                    PlainText("Rifle");
                    break;
                case Scepter:
                    PlainText("Scepter");
                    break;
                case Shield:
                    PlainText("Shield");
                    break;
                case Shortbow:
                    PlainText("Shortbow");
                    break;
                case SmallBundle:
                    PlainText("Small Bundle");
                    break;
                case Spear:
                    PlainText("Spear");
                    break;
                case Staff:
                    PlainText("Staff");
                    break;
                case Sword:
                    PlainText("Sword");
                    break;
                case Torch:
                    PlainText("Torch");
                    break;
                case Toy:
                    PlainText("Toy");
                    break;
                case ToyTwoHanded:
                    PlainText("Toy");
                    break;
                case Trident:
                    PlainText("Trident");
                    break;
                case Warhorn:
                    PlainText("Warhorn");
                    break;
            }

            RequiredLevel();

            Description();

            InBank();
            if (weapon.StatChoices.Count > 0)
            {
                PlainText("Double-click to select stats.");
            }

            if (weapon.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void Print()
        {
            Header();
            Description();

            InBank();
            if (item.Flags.Unique)
            {
                PlainText("Unique");
            }

            AccountBound();
            SoulBound();
            VendorValue();
        }

        void Header()
        {
            FlowPanel header = new()
            {
                Parent = layout,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                ControlPadding = new Vector2(5f),
                Width = layout.Width,
                Height = 50
            };

            ItemImage icon = new(item)
            {
                Parent = header
            };

            ItemName name = new(item, upgrades)
            {
                Parent = header,
                Width = layout.Width - 55,
                Height = 50,
                VerticalAlignment = VerticalAlignment.Middle,
                Font = GameService.Content.DefaultFont18,
                WrapText = true,
                SuffixItem = item.SuffixItem(upgrades) ?? item.SecondarySuffixItem(upgrades)
            };

            name.Text = name.Text.Replace(" ", "  ");
        }

        Control? WeaponStrength(Weapon weapon)
        {
            FormattedLabelBuilder? builder = new FormattedLabelBuilder()
                .SetWidth(layout.Width)
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
            label.Parent = layout;
            return label;
        }

        Control? Defense(int defense)
        {
            if (defense == 0)
            {
                return null;
            }

            return PlainText($"Defense: {defense:N0}");
        }

        Control? Attributes(IDictionary<Extensible<AttributeName>, int> attributes)
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

            return PlainText(builder.ToString());
        }

        Control Effect(Effect effect)
        {
            FlowPanel panel = new()
            {
                Parent = layout,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                Width = layout.Width,
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

        Control? Description(bool finalNewLine = false)
        {
            if (string.IsNullOrEmpty(item.Description))
            {
                return null;
            }

            Panel container = new() { Parent = layout, Width = layout.Width, HeightSizingMode = SizingMode.AutoSize };

            FormattedLabelBuilder builder = new FormattedLabelBuilder()
                .SetWidth(layout.Width - 10)
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

        Control? Mini(int miniatureId)
        {
            // TODO: mini unlock status
            return null;
        }

        Control? ItemSkin(int skinId)
        {
            // TODO: skin unlock status
            return null;
        }

        Control? ItemRarity()
        {
            if (item.Rarity == Rarity.Basic)
            {
                return null;
            }

            return PlainText($"\r\n{item.Rarity}", ItemColors.Rarity(item.Rarity));
        }

        Control? RequiredLevel()
        {
            if (item.Level == 0)
            {
                return null;
            }

            return PlainText($"Required Level: {item.Level}");
        }

        Control? InBank()
        {
            return null;
        }

        Control? AccountBound()
        {
            if (item is Currency or Service)
            {
                return null;
            }

            if (item.Flags.AccountBound)
            {
                return PlainText("Account Bound on Acquire");
            }

            if (item.Flags.AccountBindOnUse)
            {
                return PlainText("Account Bound on Use");
            }

            return null;
        }

        Control? SoulBound()
        {
            if (item is Currency or Service)
            {
                return null;
            }

            if (item.Flags.Soulbound)
            {
                return PlainText("Soulbound on Acquire");
            }

            if (item.Flags.SoulbindOnUse)
            {
                return PlainText("Soulbound on Use");
            }

            return null;
        }

        Control? VendorValue()
        {
            Coin vendorValue = item.VendorValue;
            if (vendorValue == Coin.Zero || item.Flags.NoSell)
            {
                return null;
            }

            FormattedLabelBuilder? builder = new FormattedLabelBuilder()
                .SetWidth(layout.Width)
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
            label.Parent = layout;
            label.Width = layout.Width;

            return label;
        }

        Label PlainText(string text, Color? textColor = null)
        {
            return new Label
            {
                Parent = layout,
                Width = layout.Width,
                AutoSizeHeight = true,
                Text = text,
                TextColor = textColor.GetValueOrDefault(Color.White),
                Font = GameService.Content.DefaultFont16,
                WrapText = true
            };
        }
    }

    private string AttributeName(Extensible<AttributeName> stat)
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
}