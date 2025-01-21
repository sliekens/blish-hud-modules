using System.Text;

using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.Common;
using SL.Common.Controls;
using SL.Common.ModelBinding;

using Color = Microsoft.Xna.Framework.Color;
using Container = Blish_HUD.Controls.Container;
using Currency = GuildWars2.Items.Currency;
using Item = GuildWars2.Items.Item;

namespace SL.ChatLinks.UI.Tabs.Items.Tooltips;

public sealed class ItemTooltipView(ItemTooltipViewModel viewModel) : View, ITooltipView
{
    private static readonly Color Gray = new Color(0x99, 0x99, 0x99);
    private static readonly Color ActiveBuffColor = new Color(0x55, 0x99, 0xFF);
    private readonly FlowPanel _layout = new()
    {
        FlowDirection = ControlFlowDirection.SingleTopToBottom,
        Width = 350,
        HeightSizingMode = SizingMode.AutoSize,
    };

    public ItemTooltipViewModel ViewModel { get; } = viewModel;

    protected override async Task<bool> Load(IProgress<string> progress)
    {
        await ViewModel.Load(progress);
        return true;
    }

    private void PrintArmor(Armor armor)
    {
        PrintHeader(armor);
        PrintAttributes(armor.Attributes.ToDictionary(
            stat => ViewModel.AttributeName(stat.Key),
            stat => stat.Value
        ));

        PrintUpgrades();
        PrintItemSkin();
        PrintItemRarity(armor.Rarity);
        PrintWeightClass(armor.WeightClass);
        switch (armor)
        {
            case Boots:
                PrintPlainText("Foot Armor");
                break;
            case Coat:
                PrintPlainText("Chest Armor");
                break;
            case Gloves:
                PrintPlainText("Hand Armor");
                break;
            case Helm:
            case HelmAquatic:
                PrintPlainText("Head Armor");
                break;
            case Leggings:
                PrintPlainText("Leg Armor");
                break;
            case Shoulders:
                PrintPlainText("Shoulder Armor");
                break;
        }

        PrintRequiredLevel(armor.Level);
        PrintDescription(armor.Description);
        PrintInBank();
        PrintStatChoices(armor);
        PrintUniqueness(armor);
        PrintItemBinding(armor);
        PrintVendorValue(armor);
    }

    private void PrintBackpack(Backpack back)
    {
        PrintHeader(back);
        PrintAttributes(back.Attributes.ToDictionary(
            stat => ViewModel.AttributeName(stat.Key),
            stat => stat.Value
        ));

        PrintUpgrades();

        PrintItemSkin();

        PrintItemRarity(back.Rarity);
        PrintPlainText("Back Item");
        PrintRequiredLevel(back.Level);
        PrintDescription(back.Description);
        PrintInBank();
        PrintStatChoices(back);
        PrintUniqueness(back);
        PrintItemBinding(back);
        PrintVendorValue(back);
    }

    private void PrintBag(Bag bag)
    {
        PrintHeader(bag);
        PrintDescription(bag.Description);
        PrintInBank();
        PrintUniqueness(bag);
        PrintItemBinding(bag);
        PrintVendorValue(bag);
    }

    private void PrintConsumable(Consumable consumable)
    {
        PrintHeader(consumable);
        switch (consumable)
        {
            case Currency or Service:
                PrintPlainText("Takes effect immediately upon receipt.");
                break;
            default:
                PrintPlainText("Double-click to consume.");
                break;
        }

        switch (consumable)
        {
            case Food { Effect: not null } food:
                PrintEffect(food.Effect);
                break;
            case Utility { Effect: not null } utility:
                PrintEffect(utility.Effect);
                break;
            case Service { Effect: not null } service:
                PrintEffect(service.Effect);
                break;
            case GenericConsumable { Effect: not null } generic:
                PrintEffect(generic.Effect);
                break;
        }

        PrintDescription(consumable.Description);

        switch (consumable)
        {
            case Currency or Service:
                PrintPlainText(string.IsNullOrEmpty(consumable.Description) ? "Service" : "\r\nService");
                break;
            case Transmutation transmutation:
                PrintItemSkin();
                PrintPlainText("\r\nConsumable");
                break;
            case Booze:
                PrintPlainText("""

                               Excessive alcohol consumption will result in intoxication.

                               Consumable          
                               """);
                break;
            case ContentUnlocker unlocker:
                if (!string.IsNullOrEmpty(unlocker.Description))
                {
                    PrintPlainText(" ");
                }

                if (ViewModel.DefaultLocked)
                {
                    if (ViewModel.UnlocksAvailable)
                    {
                        if (ViewModel.Unlocked!.Value)
                        {
                            PrintPlainText("""
                                           You already have that content unlocked.
                                           
                                           """, Color.Red);
                        }

                    }
                    else
                    {
                        PrintPlainText("""
                            Grant 'unlocks' permission in settings to see unlock status
                            
                            """, Gray);
                    }
                }
                else
                {
                    PrintPlainText("""
                        Unlock status unavailable.
                        
                        """, Gray);
                }

                PrintPlainText("Consumable");
                break;
            default:
                PrintPlainText(string.IsNullOrEmpty(consumable.Description) ? "Consumable" : "\r\nConsumable");
                break;
        }

        PrintRequiredLevel(consumable.Level);
        PrintInBank();
        PrintUniqueness(consumable);
        PrintItemBinding(consumable);
        PrintVendorValue(consumable);
    }

    private void PrintContainer(GuildWars2.Items.Container container)
    {
        PrintHeader(container);
        PrintDescription(container.Description);
        PrintPlainText(string.IsNullOrEmpty(container.Description) ? "Consumable" : "\r\nConsumable");
        PrintInBank();
        PrintUniqueness(container);
        PrintItemBinding(container);
        PrintVendorValue(container);
    }

    private void PrintCraftingMaterial(CraftingMaterial craftingMaterial)
    {
        PrintHeader(craftingMaterial);
        PrintDescription(craftingMaterial.Description);
        PrintInBank();
        PrintUniqueness(craftingMaterial);
        PrintItemBinding(craftingMaterial);
        PrintVendorValue(craftingMaterial);
    }

    private void PrintGatheringTool(GatheringTool gatheringTool)
    {
        PrintHeader(gatheringTool);
        PrintDescription(gatheringTool.Description);
        PrintInBank();
        PrintUniqueness(gatheringTool);
        PrintItemBinding(gatheringTool);
        PrintVendorValue(gatheringTool);
    }

    private void PrintTrinket(Trinket trinket)
    {
        PrintHeader(trinket);
        PrintAttributes(trinket.Attributes.ToDictionary(
            stat => ViewModel.AttributeName(stat.Key),
            stat => stat.Value
        ));

        PrintUpgrades();
        PrintItemRarity(trinket.Rarity);

        switch (trinket)
        {
            case Accessory:
                PrintPlainText("Accessory");
                break;
            case Amulet:
                PrintPlainText("Amulet");
                break;
            case Ring:
                PrintPlainText("Ring");
                break;
        }

        PrintRequiredLevel(trinket.Level);
        PrintDescription(trinket.Description);
        PrintInBank();
        PrintStatChoices(trinket);
        PrintUniqueness(trinket);
        PrintItemBinding(trinket);
        PrintVendorValue(trinket);
    }

    private void PrintGizmo(Gizmo gizmo)
    {
        PrintHeader(gizmo);
        PrintDescription(gizmo.Description, gizmo.Level > 0);

        if (ViewModel.DefaultLocked)
        {
            if (ViewModel.UnlocksAvailable)
            {
                if (ViewModel.Unlocked!.Value)
                {
                    PrintPlainText("""

                        Novelty Unlocked
                        """);
                }
                else
                {
                    PrintPlainText("""

                        Novelty Locked
                        """, Gray);
                }
            }
            else
            {
                PrintPlainText("""

                    Grant 'unlocks' permission in settings to see unlock status
                    """, Gray);
            }

            PrintPlainText("""

                Consumable
                """);
        }

        PrintRequiredLevel(gizmo.Level);
        PrintInBank();
        PrintUniqueness(gizmo);
        PrintItemBinding(gizmo);
        PrintVendorValue(gizmo);
    }

    private void PrintJadeTechModule(JadeTechModule jadeTechModule)
    {
        PrintHeader(jadeTechModule);
        PrintDescription(jadeTechModule.Description);
        PrintItemRarity(jadeTechModule.Rarity);
        PrintPlainText("Module");
        PrintRequiredLevel(jadeTechModule.Level);
        PrintPlainText("Required Mastery: Jade Bots");
        PrintInBank();
        PrintUniqueness(jadeTechModule);
        PrintItemBinding(jadeTechModule);
        PrintVendorValue(jadeTechModule);
    }

    private void PrintMiniature(Miniature miniature)
    {
        PrintHeader(miniature);
        PrintDescription(miniature.Description);
        PrintMini(miniature.MiniatureId);
        PrintPlainText("Mini");
        PrintInBank();
        PrintUniqueness(miniature);
        PrintItemBinding(miniature);
        PrintVendorValue(miniature);
    }

    private void PrintPowerCore(PowerCore powerCore)
    {
        PrintHeader(powerCore);
        PrintDescription(powerCore.Description, true);
        PrintItemRarity(powerCore.Rarity);
        PrintPlainText("Power Core");
        PrintRequiredLevel(powerCore.Level);
        PrintInBank();
        PrintUniqueness(powerCore);
        PrintItemBinding(powerCore);
        PrintVendorValue(powerCore);
    }

    private void PrintRelic(Relic relic)
    {
        PrintHeader(relic);
        PrintDescription(relic.Description, true);
        PrintItemRarity(relic.Rarity);
        PrintPlainText("Relic");
        PrintRequiredLevel(relic.Level);
        PrintInBank();
        PrintUniqueness(relic);
        PrintItemBinding(relic);
        PrintVendorValue(relic);
    }

    private void PrintSalvageTool(SalvageTool salvageTool)
    {
        PrintHeader(salvageTool);
        PrintPlainText(" ");
        PrintItemRarity(salvageTool.Rarity);
        PrintPlainText("Consumable");
        PrintDescription(salvageTool.Description);
        PrintInBank();
        PrintUniqueness(salvageTool);
        PrintItemBinding(salvageTool);
        PrintVendorValue(salvageTool);
    }

    private void PrintTrophy(Trophy trophy)
    {
        PrintHeader(trophy);
        PrintDescription(trophy.Description);
        PrintPlainText("Trophy");
        PrintInBank();
        PrintUniqueness(trophy);
        PrintItemBinding(trophy);
        PrintVendorValue(trophy);
    }

    private void PrintUpgradeComponent(UpgradeComponent upgradeComponent)
    {
        PrintHeader(upgradeComponent);
        if (upgradeComponent is Rune rune)
        {
            PrintBonuses(rune.Bonuses ?? []);
        }
        else if (upgradeComponent.Buff is { Description.Length: > 0 })
        {
            PrintBuff(upgradeComponent.Buff);
        }
        else
        {
            PrintAttributes(upgradeComponent.Attributes.ToDictionary(
                stat => ViewModel.AttributeName(stat.Key),
                stat => stat.Value
            ));
        }

        PrintDescription(upgradeComponent.Description);
        PrintRequiredLevel(upgradeComponent.Level);
        PrintInBank();
        PrintUniqueness(upgradeComponent);
        PrintItemBinding(upgradeComponent);
        PrintVendorValue(upgradeComponent);
    }

    private void PrintWeapon(Weapon weapon)
    {
        PrintHeader(weapon);
        PrintWeaponStrength(weapon);
        PrintDefense(weapon.Defense);
        PrintAttributes(weapon.Attributes.ToDictionary(
            stat => ViewModel.AttributeName(stat.Key),
            stat => stat.Value
        ));

        PrintUpgrades();
        PrintItemSkin();
        PrintItemRarity(weapon.Rarity);
        switch (weapon)
        {
            case Axe:
                PrintPlainText("Axe");
                break;
            case Dagger:
                PrintPlainText("Dagger");
                break;
            case Focus:
                PrintPlainText("Focus");
                break;
            case Greatsword:
                PrintPlainText("Greatsword");
                break;
            case Hammer:
                PrintPlainText("Hammer");
                break;
            case HarpoonGun:
                PrintPlainText("Harpoon Gun");
                break;
            case LargeBundle:
                PrintPlainText("Large Bundle");
                break;
            case Longbow:
                PrintPlainText("Longbow");
                break;
            case Mace:
                PrintPlainText("Mace");
                break;
            case Pistol:
                PrintPlainText("Pistol");
                break;
            case Rifle:
                PrintPlainText("Rifle");
                break;
            case Scepter:
                PrintPlainText("Scepter");
                break;
            case Shield:
                PrintPlainText("Shield");
                break;
            case Shortbow:
                PrintPlainText("Shortbow");
                break;
            case SmallBundle:
                PrintPlainText("Small Bundle");
                break;
            case Spear:
                PrintPlainText("Spear");
                break;
            case Staff:
                PrintPlainText("Staff");
                break;
            case Sword:
                PrintPlainText("Sword");
                break;
            case Torch:
                PrintPlainText("Torch");
                break;
            case Toy:
            case ToyTwoHanded:
                PrintPlainText("Toy");
                break;
            case Trident:
                PrintPlainText("Trident");
                break;
            case Warhorn:
                PrintPlainText("Warhorn");
                break;
        }

        PrintRequiredLevel(weapon.Level);
        PrintDescription(weapon.Description);
        PrintInBank();
        PrintStatChoices(weapon);
        PrintUniqueness(weapon);
        PrintItemBinding(weapon);
        PrintVendorValue(weapon);
    }

    private void Print(Item item)
    {
        PrintHeader(item);
        PrintDescription(item.Description);
        PrintInBank();
        PrintUniqueness(item);
        PrintItemBinding(item);
        PrintVendorValue(item);
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

    public void PrintHeader(Item item)
    {
        FlowPanel header = new()
        {
            Parent = _layout,
            FlowDirection = ControlFlowDirection.SingleLeftToRight,
            ControlPadding = new Vector2(5f),
            Width = _layout.Width,
            Height = 50
        };

        Image icon = new()
        {
            Parent = header,
            Texture = ViewModel.GetIcon(ViewModel.Item),
            Size = new Point(50)
        };

        Label name = new()
        {
            Parent = header,
            TextColor = ViewModel.ItemNameColor,
            Width = _layout.Width - 55,
            Height = 50,
            VerticalAlignment = VerticalAlignment.Middle,
            Font = GameService.Content.DefaultFont18,
            WrapText = true,
        };

        Binder.Bind(ViewModel, vm => vm.ItemName, name);

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

    public void PrintUpgrades()
    {
        foreach (var slot in ViewModel.UpgradesSlots)
        {
            FormattedLabelBuilder builder = new FormattedLabelBuilder()
                .SetWidth(_layout.Width)
                .AutoSizeHeight()
                .Wrap();

            if (slot.UpgradeComponent is not null)
            {
                builder
                    .CreatePart("\r\n", _ => { })
                    .CreatePart(" " + slot.UpgradeComponent.Name, part =>
                    {
                        if (!string.IsNullOrEmpty(slot.UpgradeComponent.IconHref))
                        {
                            part.SetPrefixImage(ViewModel.GetIcon(slot.UpgradeComponent));
                            part.SetPrefixImageSize(new Point(16));
                        }

                        part.SetFontSize(ContentService.FontSize.Size16);
                        part.SetTextColor(ActiveBuffColor);
                    });

                if (slot.UpgradeComponent is Rune rune)
                {
                    foreach ((string? bonus, int ordinal) in (rune.Bonuses ?? []).Select((value, index) => (value, index + 1)))
                    {
                        builder.CreatePart($"\r\n({ordinal:0}): {bonus}", part =>
                        {
                            part.SetFontSize(ContentService.FontSize.Size16);
                            part.SetTextColor(Gray);
                        });
                    }
                }
                else if (slot.UpgradeComponent.Buff is { Description.Length: > 0 })
                {
                    builder.CreatePart("\r\n", part => part.SetFontSize(ContentService.FontSize.Size16));
                    builder.AddMarkup(slot.UpgradeComponent.Buff.Description, ActiveBuffColor);
                }
                else
                {
                    foreach (KeyValuePair<Extensible<AttributeName>, int> stat in slot.UpgradeComponent.Attributes)
                    {
                        builder.CreatePart("\r\n", part => part.SetFontSize(ContentService.FontSize.Size16));
                        builder.CreatePart($"+{stat.Value:N0} {ViewModel.AttributeName(stat.Key)}", part =>
                        {
                            part.SetFontSize(ContentService.FontSize.Size16);
                            part.SetTextColor(ActiveBuffColor);
                        });
                    }
                }
            }
            else
            {
                switch (slot.Type)
                {
                    case UpgradeSlotType.Infusion:
                        builder
                            .CreatePart("\r\n", _ => { })
                            .CreatePart(" Unused Infusion Slot", part =>
                            {
                                part.SetPrefixImage(Resources.Texture("unused_infusion_slot.png"));
                                part.SetPrefixImageSize(new Point(16));
                                part.SetFontSize(ContentService.FontSize.Size16);
                            });
                        break;
                    case UpgradeSlotType.Enrichment:
                        builder
                            .CreatePart("\r\n", _ => { })
                            .CreatePart(" Unused Enrichment Slot", part =>
                            {
                                part.SetPrefixImage(Resources.Texture("unused_enrichment_slot.png"));
                                part.SetPrefixImageSize(new Point(16));
                                part.SetFontSize(ContentService.FontSize.Size16);
                            });
                        break;
                    case UpgradeSlotType.Default:
                    default:
                        builder
                            .CreatePart("\r\n", _ => { })
                            .CreatePart(" Unused Upgrade Slot", part =>
                            {
                                part.SetPrefixImage(Resources.Texture("unused_upgrade_slot.png"));
                                part.SetPrefixImageSize(new Point(16));
                                part.SetFontSize(ContentService.FontSize.Size16);
                            });
                        break;
                }
            }

            var upgradeSlot = builder.Build();
            upgradeSlot.Parent = _layout;
        }

    }

    public void PrintItemSkin()
    {
        if (ViewModel.DefaultLocked)
        {
            if (ViewModel.UnlocksAvailable)
            {
                if (ViewModel.Unlocked!.Value)
                {
                    PrintPlainText("\r\nSkin Unlocked");
                }
                else
                {
                    PrintPlainText("\r\nSkin Locked", Gray);
                }
            }
            else
            {
                PrintPlainText("\r\nGrant 'unlocks' permission in settings to see unlock status", Gray);
            }
        }
    }

    public void PrintItemRarity(Extensible<Rarity> rarity)
    {
        if (rarity == Rarity.Basic)
        {
            PrintPlainText(" ");
        }
        else
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

    public void PrintVendorValue(Item _)
    {
        var totalValue = ViewModel.TotalVendorValue;
        if (totalValue == Coin.Zero || ViewModel.Item.Flags.NoSell)
        {
            return;
        }

        FormattedLabelBuilder? builder = new FormattedLabelBuilder()
            .SetWidth(_layout.Width)
            .AutoSizeHeight();

        if (totalValue.Amount >= 10_000)
        {
            FormattedLabelPartBuilder? gold = builder.CreatePart(totalValue.Gold.ToString("N0"));
            gold.SetTextColor(new Color(0xDD, 0xBB, 0x44));
            gold.SetFontSize(ContentService.FontSize.Size16);
            gold.SetSuffixImage(AsyncTexture2D.FromAssetId(156904));
            gold.SetSuffixImageSize(new Point(20));
            builder.CreatePart(gold);
            builder.CreatePart("  ", _ => { });
        }

        if (totalValue.Amount >= 100)
        {
            FormattedLabelPartBuilder? silver = builder.CreatePart(totalValue.Silver.ToString("N0"));
            silver.SetTextColor(new Color(0xC0, 0xC0, 0xC0));
            silver.SetFontSize(ContentService.FontSize.Size16);
            silver.SetSuffixImage(AsyncTexture2D.FromAssetId(156907));
            silver.SetSuffixImageSize(new Point(20));
            builder.CreatePart(silver);
            builder.CreatePart("  ", _ => { });
        }

        FormattedLabelPartBuilder? copper = builder.CreatePart(totalValue.Copper.ToString("N0"));
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
            .AddMarkup(buff.Description, ActiveBuffColor)
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

        PrintPlainText(text.ToString(), Gray);
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
                        part.SetTextColor(Gray);
                    });
                    break;
                case DamageType.Fire:
                    builder.CreatePart(" (Fire)", static part =>
                    {
                        part.SetFontSize(ContentService.FontSize.Size16);
                        part.SetTextColor(Gray);
                    });
                    break;
                case DamageType.Ice:
                    builder.CreatePart(" (Ice)", static part =>
                    {
                        part.SetFontSize(ContentService.FontSize.Size16);
                        part.SetTextColor(Gray);
                    });
                    break;
                case DamageType.Lightning:
                    builder.CreatePart(" (Lightning)", static part =>
                    {
                        part.SetFontSize(ContentService.FontSize.Size16);
                        part.SetTextColor(Gray);
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
        switch (ViewModel.Item)
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
                Print(ViewModel.Item);
                break;
        }

        _layout.Parent = buildPanel;
    }
}