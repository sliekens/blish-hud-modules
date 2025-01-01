using GuildWars2.Items;

using SL.Common;
using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public sealed class UpgradeSlotsViewModel : ViewModel
{
	public event Action? UpgradesChanged;

	public Item Item { get; }

	public IReadOnlyDictionary<int, UpgradeComponent> UpgradeComponents { get; }

	public IReadOnlyList<(UpgradeSlotViewModel Slot, UpgradeComponentsListViewModel Options)> UpgradeSlots { get; }

	public IReadOnlyList<(UpgradeSlotViewModel Slot, UpgradeComponentsListViewModel Options)> InfusionSlots { get; }

	public int? SuffixItemId => UpgradeSlots.FirstOrDefault().Slot?.SelectedUpgradeComponent?.Id;

	public int? SecondarySuffixItemId => UpgradeSlots.Skip(1).FirstOrDefault().Slot?.SelectedUpgradeComponent?.Id;

	public UpgradeSlotsViewModel(Item item,
		IReadOnlyDictionary<int, UpgradeComponent> upgradeComponents)
	{
		Item = item;
		UpgradeComponents = upgradeComponents;
		UpgradeSlots = SetupUpgradeSlots(item);
		InfusionSlots = SetupInfusionSlots(item);
	}

	public UpgradeComponent? EffctiveSuffixItem()
	{
		return UpgradeSlots.Select(pair => pair.Slot.EffectiveUpgrade)
			.FirstOrDefault(upgrade => upgrade is not null);
	}

	public UpgradeSlotViewModel? UpgradeSlot1 => UpgradeSlots?.FirstOrDefault().Slot;

	public UpgradeSlotViewModel? UpgradeSlot2 => UpgradeSlots?.Skip(1).FirstOrDefault().Slot;

	public IReadOnlyList<InfusionSlot> Infusions()
	{
		return InfusionSlots
			.Select(pair => new InfusionSlot
			{
				ItemId = pair.Slot.EffectiveUpgrade?.Id,
				Flags = new InfusionSlotFlags
				{
					Infusion = pair.Slot.Type == UpgradeSlotType.Infusion,
					Enrichment = pair.Slot.Type == UpgradeSlotType.Enrichment,
					Other = []
				}
			})
			.ToList()
			.AsReadOnly();
	}

	private IReadOnlyList<(UpgradeSlotViewModel Slot, UpgradeComponentsListViewModel Options)> SetupUpgradeSlots(Item item)
	{
		if (item is not IUpgradable upgradable)
		{
			return [];
		}

		var upgradeSlots = new List<(UpgradeSlotViewModel Slot, UpgradeComponentsListViewModel Options)>(upgradable.UpgradeSlotCount);
		foreach (var upgradeComponentId in upgradable.UpgradeSlots)
		{
			UpgradeSlotViewModel slotViewModel = new(UpgradeSlotType.Default)
			{
				DefaultUpgradeComponent = DefaultUpgradeComponent(upgradeComponentId)
			};

			slotViewModel.PropertyChanged += (sender, args) =>
			{
				OnUpgradesChanged();
			};

			UpgradeComponentsListViewModel optionsViewModel = new(
				item,
				UpgradeSlotType.Default,
				UpgradeComponents
			);

			optionsViewModel.Selected += upgradeComponent =>
			{
				slotViewModel.SelectedUpgradeComponent = slotViewModel.SelectedUpgradeComponent != upgradeComponent
					? upgradeComponent
					: null;
			};

			upgradeSlots.Add((slotViewModel, optionsViewModel));
		}

		return upgradeSlots.AsReadOnly();
	}

	private IReadOnlyList<(UpgradeSlotViewModel Slot, UpgradeComponentsListViewModel Options)> SetupInfusionSlots(Item item)
	{
		if (item is not IUpgradable upgradable)
		{
			return [];
		}

		var upgradeSlots = new List<(UpgradeSlotViewModel Slot, UpgradeComponentsListViewModel Options)>(upgradable.InfusionSlotCount);
		foreach (var infusionSlot in upgradable.InfusionSlots)
		{
			UpgradeSlotType upgradeSlotType = infusionSlot.Flags switch
			{
				{ Infusion: true } => UpgradeSlotType.Infusion,
				{ Enrichment: true } => UpgradeSlotType.Enrichment,
				_ => UpgradeSlotType.Default
			};

			UpgradeSlotViewModel slotViewModel = new(upgradeSlotType)
			{
				DefaultUpgradeComponent = DefaultUpgradeComponent(infusionSlot.ItemId)
			};

			slotViewModel.PropertyChanged += (sender, args) =>
			{
				OnUpgradesChanged();
			};

			UpgradeComponentsListViewModel optionsViewModel = new(
				item,
				upgradeSlotType,
				UpgradeComponents
			);

			optionsViewModel.Selected += upgradeComponent =>
			{
				slotViewModel.SelectedUpgradeComponent = slotViewModel.SelectedUpgradeComponent != upgradeComponent
					? upgradeComponent
					: null;
			};

			upgradeSlots.Add((slotViewModel, optionsViewModel));
		}

		return upgradeSlots.AsReadOnly();
	}

	public UpgradeComponent? DefaultUpgradeComponent(int? upgradeComponentId)
	{
		if (upgradeComponentId.HasValue
			&& UpgradeComponents.TryGetValue(upgradeComponentId.Value, out var upgradeComponent)
		)
		{
			return upgradeComponent;

		}

		return null;
	}

	private void OnUpgradesChanged()
	{
		UpgradesChanged?.Invoke();
	}
}