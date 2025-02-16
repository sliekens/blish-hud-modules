using System.Text;

using Blish_HUD;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Hero.Equipment.Wardrobe;
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
	private static readonly Color Gray = new(0x99, 0x99, 0x99);
	private static readonly Color ActiveBuffColor = new(0x55, 0x99, 0xFF);
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
		PrintHeader();
		PrintAttributes(armor.Attributes.ToDictionary(
			stat => stat.Key.ToString(),
			stat => stat.Value
		));

		PrintUpgrades();
		PrintItemSkin();
		PrintItemRarity(armor.Rarity);
		PrintWeightClass(armor.WeightClass);
		switch (armor)
		{
			case Boots:
				PrintPlainText(ViewModel.Localizer["Foot Armor"]);
				break;
			case Coat:
				PrintPlainText(ViewModel.Localizer["Chest Armor"]);
				break;
			case Gloves:
				PrintPlainText(ViewModel.Localizer["Hand Armor"]);
				break;
			case Helm:
			case HelmAquatic:
				PrintPlainText(ViewModel.Localizer["Head Armor"]);
				break;
			case Leggings:
				PrintPlainText(ViewModel.Localizer["Leg Armor"]);
				break;
			case Shoulders:
				PrintPlainText(ViewModel.Localizer["Shoulder Armor"]);
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
		PrintHeader();
		PrintAttributes(back.Attributes.ToDictionary(
			stat => ViewModel.Localizer[stat.Key.ToString()].ToString(),
			stat => stat.Value
		));

		PrintUpgrades();

		PrintItemSkin();

		PrintItemRarity(back.Rarity);
		PrintPlainText(ViewModel.Localizer["Back Item"]);
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
		PrintHeader();
		PrintDescription(bag.Description);
		PrintInBank();
		PrintUniqueness(bag);
		PrintItemBinding(bag);
		PrintVendorValue(bag);
	}

	private void PrintConsumable(Consumable consumable)
	{
		PrintHeader();
		switch (consumable)
		{
			case Currency or Service:
				PrintPlainText(ViewModel.Localizer["Takes effect immediately upon receipt"]);
				break;
			default:
				PrintPlainText(ViewModel.Localizer["Double-click to consume"]);
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
				if (string.IsNullOrEmpty(consumable.Description))
				{
					PrintPlainText(ViewModel.Localizer["Service"]);
				}
				else
				{
					PrintPlainText("\r\n" + ViewModel.Localizer["Service"]);
				}
				break;
			case Transmutation:
				PrintTransmutation();
				PrintPlainText("\r\n" + ViewModel.Localizer["Consumable"]);
				break;
			case Booze:
				PrintPlainText($"""

                               {ViewModel.Localizer["Excessive alcohol consumption will result in intoxication"]}

                               {ViewModel.Localizer["Consumable"]}
                               """);
				break;
			case ContentUnlocker unlocker:
				if (!string.IsNullOrEmpty(unlocker.Description))
				{
					PrintPlainText(" ");
				}

				if (ViewModel.DefaultLocked)
				{
					if (ViewModel.Unlocked.HasValue)
					{
						if (ViewModel.Unlocked.Value)
						{
							PrintPlainText($"""
                                           {ViewModel.Localizer["You already have that content unlocked"]}

                                           """, Color.Red);
						}

					}
					else
					{
						PrintPlainText($"""
                                        {ViewModel.AuthorizationText}

                                        """, Gray);
					}
				}
				else
				{
					PrintPlainText($"""
                                   {ViewModel.Localizer["Unlock status unknown"]}

                                   """, Gray);
				}

				PrintPlainText(ViewModel.Localizer["Consumable"]);
				break;
			case Dye unlocker:
				if (!string.IsNullOrEmpty(unlocker.Description))
				{
					PrintPlainText(" ");
				}

				if (ViewModel.DefaultLocked)
				{
					if (ViewModel.Unlocked.HasValue)
					{
						if (ViewModel.Unlocked.Value)
						{
							PrintPlainText("""
                                           You have already unlocked this dye!

                                           """, Color.Red);
						}

					}
					else
					{
						PrintPlainText($"""
                                        {ViewModel.AuthorizationText}

                                        """, Gray);
					}
				}
				else
				{
					PrintPlainText($"""
                                   {ViewModel.Localizer["Unlock status unknown"]}

                                   """, Gray);
				}

				PrintPlainText(ViewModel.Localizer["Consumable"]);
				break;
			case GliderSkinUnlocker unlocker:
				if (!string.IsNullOrEmpty(unlocker.Description))
				{
					PrintPlainText(" ");
				}

				if (ViewModel.DefaultLocked)
				{
					if (ViewModel.Unlocked.HasValue)
					{
						if (ViewModel.Unlocked.Value)
						{
							PrintPlainText("""
                                           You have already unlocked this glider!

                                           """, Color.Red);
						}

					}
					else
					{
						PrintPlainText($"""
                                        {ViewModel.AuthorizationText}

                                        """, Gray);
					}
				}
				else
				{
					PrintPlainText($"""
                                   {ViewModel.Localizer["Unlock status unknown"]}

                                   """, Gray);
				}

				PrintPlainText(ViewModel.Localizer["Consumable"]);
				break;
			case JadeBotSkinUnlocker unlocker:
				if (!string.IsNullOrEmpty(unlocker.Description))
				{
					PrintPlainText(" ");
				}

				if (ViewModel.DefaultLocked)
				{
					if (ViewModel.Unlocked.HasValue)
					{
						if (ViewModel.Unlocked.Value)
						{
							PrintPlainText("""
                                           You have already unlocked this Jade Bot!

                                           """, Color.Red);
						}

					}
					else
					{
						PrintPlainText($"""
                                        {ViewModel.AuthorizationText}

                                        """, Gray);
					}
				}
				else
				{
					PrintPlainText($"""
                                   {ViewModel.Localizer["Unlock status unknown"]}

                                   """, Gray);
				}

				PrintPlainText(ViewModel.Localizer["Consumable"]);
				break;
			case MistChampionSkinUnlocker unlocker:
				if (!string.IsNullOrEmpty(unlocker.Description))
				{
					PrintPlainText(" ");
				}

				if (ViewModel.DefaultLocked)
				{
					if (ViewModel.Unlocked.HasValue)
					{
						if (ViewModel.Unlocked.Value)
						{
							PrintPlainText("""
                                           You have already unlocked this outfit!

                                           """, Color.Red);
						}

					}
					else
					{
						PrintPlainText($"""
                                        {ViewModel.AuthorizationText}

                                        """, Gray);
					}
				}
				else
				{
					PrintPlainText($"""
                                   {ViewModel.Localizer["Unlock status unknown"]}

                                   """, Gray);
				}

				PrintPlainText(ViewModel.Localizer["Consumable"]);
				break;
			case OutfitUnlocker unlocker:
				if (!string.IsNullOrEmpty(unlocker.Description))
				{
					PrintPlainText(" ");
				}

				if (ViewModel.DefaultLocked)
				{
					if (ViewModel.Unlocked.HasValue)
					{
						if (ViewModel.Unlocked.Value)
						{
							PrintPlainText($"""
                                           {ViewModel.Localizer["You have already unlocked this outfit"]}

                                           """, Color.Red);
						}

					}
					else
					{
						PrintPlainText($"""
                                        {ViewModel.AuthorizationText}

                                        """, Gray);
					}
				}
				else
				{
					PrintPlainText($"""
                                   {ViewModel.Localizer["Unlock status unknown"]}

                                   """, Gray);
				}

				PrintPlainText(ViewModel.Localizer["Consumable"]);
				break;
			case RecipeSheet unlocker:
				if (!string.IsNullOrEmpty(unlocker.Description))
				{
					PrintPlainText(" ");
				}

				if (ViewModel.DefaultLocked)
				{
					if (ViewModel.Unlocked.HasValue)
					{
						if (ViewModel.Unlocked.Value)
						{
							PrintPlainText($"""
                                           {ViewModel.UnlockedText}

                                           """, ViewModel.UnlockedTextColor);
						}

					}
					else
					{
						PrintPlainText($"""
                                        {ViewModel.AuthorizationText}

                                        """, Gray);
					}
				}
				else
				{
					PrintPlainText($"""
                                   {ViewModel.Localizer["Unlock status unknown"]}

                                   """, Gray);
				}

				PrintPlainText(ViewModel.Localizer["Consumable"]);
				break;
			default:
				if (string.IsNullOrEmpty(consumable.Description))
				{
					PrintPlainText(ViewModel.Localizer["Consumable"]);
				}
				else
				{
					PrintPlainText("\r\n" + ViewModel.Localizer["Consumable"]);
				}
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
		PrintHeader();
		PrintDescription(container.Description);
		if (string.IsNullOrEmpty(container.Description))
		{
			PrintPlainText(ViewModel.Localizer["Consumable"]);
		}
		else
		{
			PrintPlainText("\r\n" + ViewModel.Localizer["Consumable"]);
		}
		PrintInBank();
		PrintUniqueness(container);
		PrintItemBinding(container);
		PrintVendorValue(container);
	}

	private void PrintCraftingMaterial(CraftingMaterial craftingMaterial)
	{
		PrintHeader();
		PrintDescription(craftingMaterial.Description);
		PrintInBank();
		PrintUniqueness(craftingMaterial);
		PrintItemBinding(craftingMaterial);
		PrintVendorValue(craftingMaterial);
	}

	private void PrintGatheringTool(GatheringTool gatheringTool)
	{
		PrintHeader();
		PrintDescription(gatheringTool.Description);
		PrintInBank();
		PrintUniqueness(gatheringTool);
		PrintItemBinding(gatheringTool);
		PrintVendorValue(gatheringTool);
	}

	private void PrintTrinket(Trinket trinket)
	{
		PrintHeader();
		PrintAttributes(trinket.Attributes.ToDictionary(
			stat => ViewModel.Localizer[stat.Key.ToString()].ToString(),
			stat => stat.Value
		));

		PrintUpgrades();
		PrintItemRarity(trinket.Rarity);

		switch (trinket)
		{
			case Accessory:
				PrintPlainText(ViewModel.Localizer["Accessory"]);
				break;
			case Amulet:
				PrintPlainText(ViewModel.Localizer["Amulet"]);
				break;
			case Ring:
				PrintPlainText(ViewModel.Localizer["Ring"]);
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
		PrintHeader();
		PrintDescription(gizmo.Description, gizmo.Level > 0);

		if (ViewModel.DefaultLocked)
		{
			if (ViewModel.Unlocked.HasValue)
			{
				if (ViewModel.Unlocked.Value)
				{
					PrintPlainText($"""

                        {ViewModel.Localizer["Novelty Unlocked"]}
                        """);
				}
				else
				{
					PrintPlainText($"""

                        {ViewModel.Localizer["Novelty Locked"]}
                        """, Gray);
				}
			}
			else
			{
				PrintPlainText($"""

                    {ViewModel.AuthorizationText}
                    """, Gray);
			}

			PrintPlainText($"""

                {ViewModel.Localizer["Consumable"]}
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
		PrintHeader();
		PrintDescription(jadeTechModule.Description);
		PrintItemRarity(jadeTechModule.Rarity);
		PrintPlainText(ViewModel.Localizer["Module"]);
		PrintRequiredLevel(jadeTechModule.Level);
		PrintPlainText(ViewModel.Localizer["Required Mastery: Jade Bots"]);
		PrintInBank();
		PrintUniqueness(jadeTechModule);
		PrintItemBinding(jadeTechModule);
		PrintVendorValue(jadeTechModule);
	}

	private void PrintMiniature(Miniature miniature)
	{
		PrintHeader();
		PrintDescription(miniature.Description);
		if (ViewModel.DefaultLocked)
		{
			if (ViewModel.Unlocked.HasValue)
			{
				if (ViewModel.Unlocked.Value)
				{
					PrintPlainText($"""

                                    {ViewModel.Localizer["Mini Unlocked"]}

                                    """);
				}
				else
				{
					PrintPlainText($"""

                                    {ViewModel.Localizer["Mini Locked"]}

                                    """, Gray);
				}
			}
			else
			{
				PrintPlainText($"""

                                {ViewModel.AuthorizationText}

                                """, Gray);
			}
		}

		PrintPlainText(ViewModel.Localizer["Mini"]);
		PrintInBank();
		PrintUniqueness(miniature);
		PrintItemBinding(miniature);
		PrintVendorValue(miniature);
	}

	private void PrintPowerCore(PowerCore powerCore)
	{
		PrintHeader();
		PrintDescription(powerCore.Description, true);
		PrintItemRarity(powerCore.Rarity);
		PrintPlainText(ViewModel.Localizer["Power Core"]);
		PrintRequiredLevel(powerCore.Level);
		PrintInBank();
		PrintUniqueness(powerCore);
		PrintItemBinding(powerCore);
		PrintVendorValue(powerCore);
	}

	private void PrintRelic(Relic relic)
	{
		PrintHeader();
		PrintDescription(relic.Description, true);
		PrintItemRarity(relic.Rarity);
		PrintPlainText(ViewModel.Localizer["Relic"]);
		PrintRequiredLevel(relic.Level);
		PrintInBank();
		PrintUniqueness(relic);
		PrintItemBinding(relic);
		PrintVendorValue(relic);
	}

	private void PrintSalvageTool(SalvageTool salvageTool)
	{
		PrintHeader();
		PrintPlainText(" ");
		PrintItemRarity(salvageTool.Rarity);
		PrintPlainText(ViewModel.Localizer["Consumable"]);
		PrintDescription(salvageTool.Description);
		PrintInBank();
		PrintUniqueness(salvageTool);
		PrintItemBinding(salvageTool);
		PrintVendorValue(salvageTool);
	}

	private void PrintTrophy(Trophy trophy)
	{
		PrintHeader();
		PrintDescription(trophy.Description);
		PrintPlainText(ViewModel.Localizer["Trophy"]);
		PrintInBank();
		PrintUniqueness(trophy);
		PrintItemBinding(trophy);
		PrintVendorValue(trophy);
	}

	private void PrintUpgradeComponent(UpgradeComponent upgradeComponent)
	{
		PrintHeader();
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
				stat => ViewModel.Localizer[stat.Key.ToString()].ToString(),
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
		PrintHeader();
		PrintWeaponStrength(weapon);
		PrintDefense(weapon.Defense);
		PrintAttributes(weapon.Attributes.ToDictionary(
			stat => ViewModel.Localizer[stat.Key.ToString()].ToString(),
			stat => stat.Value
		));

		PrintUpgrades();
		PrintItemSkin();
		PrintItemRarity(weapon.Rarity);
		switch (weapon)
		{
			case Axe:
				PrintPlainText(ViewModel.Localizer["Axe"]);
				break;
			case Dagger:
				PrintPlainText(ViewModel.Localizer["Dagger"]);
				break;
			case Focus:
				PrintPlainText(ViewModel.Localizer["Focus"]);
				break;
			case Greatsword:
				PrintPlainText(ViewModel.Localizer["Greatsword"]);
				break;
			case Hammer:
				PrintPlainText(ViewModel.Localizer["Hammer"]);
				break;
			case HarpoonGun:
				PrintPlainText(ViewModel.Localizer["Harpoon Gun"]);
				break;
			case LargeBundle:
				PrintPlainText(ViewModel.Localizer["Large Bundle"]);
				break;
			case Longbow:
				PrintPlainText(ViewModel.Localizer["Longbow"]);
				break;
			case Mace:
				PrintPlainText(ViewModel.Localizer["Mace"]);
				break;
			case Pistol:
				PrintPlainText(ViewModel.Localizer["Pistol"]);
				break;
			case Rifle:
				PrintPlainText(ViewModel.Localizer["Rifle"]);
				break;
			case Scepter:
				PrintPlainText(ViewModel.Localizer["Scepter"]);
				break;
			case Shield:
				PrintPlainText(ViewModel.Localizer["Shield"]);
				break;
			case Shortbow:
				PrintPlainText(ViewModel.Localizer["Shortbow"]);
				break;
			case SmallBundle:
				PrintPlainText(ViewModel.Localizer["Small Bundle"]);
				break;
			case Spear:
				PrintPlainText(ViewModel.Localizer["Spear"]);
				break;
			case Staff:
				PrintPlainText(ViewModel.Localizer["Staff"]);
				break;
			case Sword:
				PrintPlainText(ViewModel.Localizer["Sword"]);
				break;
			case Torch:
				PrintPlainText(ViewModel.Localizer["Torch"]);
				break;
			case Toy:
			case ToyTwoHanded:
				PrintPlainText(ViewModel.Localizer["Toy"]);
				break;
			case Trident:
				PrintPlainText(ViewModel.Localizer["Trident"]);
				break;
			case Warhorn:
				PrintPlainText(ViewModel.Localizer["Warhorn"]);
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
		PrintHeader();
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

	public void PrintHeader()
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
			PrintPlainText(ViewModel.Localizer["Defense", defense]);
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

				builder.Append(ViewModel.Localizer[stat.Key, stat.Value]);
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
						builder.CreatePart($"+{stat.Value:N0} {ViewModel.Localizer[stat.Key.ToString()]}", part =>
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
							.CreatePart(" " + ViewModel.Localizer["Unused infusion slot"], part =>
							{
								part.SetPrefixImage(EmbeddedResources.Texture("unused_infusion_slot.png"));
								part.SetPrefixImageSize(new Point(16));
								part.SetFontSize(ContentService.FontSize.Size16);
							});
						break;
					case UpgradeSlotType.Enrichment:
						builder
							.CreatePart("\r\n", _ => { })
							.CreatePart(" " + ViewModel.Localizer["Unused enrichment slot"], part =>
							{
								part.SetPrefixImage(EmbeddedResources.Texture("unused_enrichment_slot.png"));
								part.SetPrefixImageSize(new Point(16));
								part.SetFontSize(ContentService.FontSize.Size16);
							});
						break;
					case UpgradeSlotType.Default:
					default:
						builder
							.CreatePart("\r\n", _ => { })
							.CreatePart(" " + ViewModel.Localizer["Unused upgrade slot"], part =>
							{
								part.SetPrefixImage(EmbeddedResources.Texture("unused_upgrade_slot.png"));
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
			if (ViewModel.Unlocked.HasValue)
			{
				if (ViewModel.Unlocked.Value)
				{
					PrintPlainText($"""

                                    {ViewModel.Localizer["Skin Unlocked"]}
                                    {ViewModel.DefaultSkin?.Name}
                                    """);
				}
				else
				{
					PrintPlainText($"""

                                    {ViewModel.Localizer["Skin Locked"]}
                                    {ViewModel.DefaultSkin?.Name}
                                    """, Gray);
				}
			}
			else
			{
				PrintPlainText($"""

                                {ViewModel.AuthorizationText}
                                {ViewModel.DefaultSkin?.Name}
                                """, Gray);
			}
		}
	}

	public void PrintTransmutation()
	{
		if (ViewModel.DefaultLocked)
		{
			if (ViewModel.Unlocked.HasValue)
			{
				if (ViewModel.Unlocked.Value)
				{
					PrintPlainText($"""

                                    {ViewModel.Localizer["Skin Unlocked"]}
                                    """);
				}
				else
				{
					PrintPlainText($"""

                                    {ViewModel.Localizer["Skin Locked"]}
                                    """, Gray);
				}
			}
			else
			{
				PrintPlainText($"""

                                {ViewModel.AuthorizationText}
                                """, Gray);
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
			PrintPlainText($"\r\n{ViewModel.Localizer[rarity.ToString()]}", ItemColors.Rarity(rarity));
		}
	}

	public void PrintWeightClass(Extensible<WeightClass> weightClass)
	{
		if (weightClass != WeightClass.Clothing)
		{
			PrintPlainText(ViewModel.Localizer[weightClass.ToString()]);
		}
	}

	public void PrintRequiredLevel(int level)
	{
		if (level > 0)
		{
			PrintPlainText(ViewModel.Localizer["Required Level", level]);
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
			PrintPlainText(ViewModel.Localizer["Account Bound on Acquire"]);
		}
		else if (item.Flags.AccountBindOnUse)
		{
			PrintPlainText(ViewModel.Localizer["Account Bound on Use"]);
		}

		if (item.Flags.Soulbound)
		{
			PrintPlainText(ViewModel.Localizer["Soulbound on Acquire"]);
		}
		else if (item.Flags.SoulbindOnUse)
		{
			PrintPlainText(ViewModel.Localizer["Soulbound on Use"]);
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
			_ = new Image()
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
		_ = new Label()
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

		builder.CreatePart(ViewModel.Localizer["Weapon Strength", weapon.MinPower, weapon.MaxPower], static part =>
		{
			part.SetFontSize(ContentService.FontSize.Size16);
		});

		Extensible<DamageType> damageType = weapon.DamageType;
		if (ViewModel.DefaultSkin is WeaponSkin defaultSkin)
		{
			damageType = defaultSkin.DamageType;
		}

		if (damageType != DamageType.Physical)
		{
			builder.CreatePart($" ({ViewModel.Localizer[damageType.ToString()]})", static part =>
			{
				part.SetFontSize(ContentService.FontSize.Size16);
				part.SetTextColor(Gray);
			});
		}

		FormattedLabel? label = builder.Build();
		label.Parent = _layout;
	}

	public void PrintStatChoices(ICombatEquipment equipment)
	{
		if (equipment.StatChoices.Count > 0)
		{
			PrintPlainText(ViewModel.Localizer["Double-click to select stats."]);
		}
	}

	public void PrintUniqueness(Item item)
	{
		if (item.Flags.Unique)
		{
			PrintPlainText(ViewModel.Localizer["Unique"]);
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