using Blish_HUD.Controls;

using GuildWars2.Hero;
using GuildWars2.Items;

using SL.Common;
using SL.Common.Controls.Items;
using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public sealed class UpgradeComponentsListViewModel(
    Item item,
    UpgradeSlotType slotType,
    ItemIcons icons,
    IReadOnlyDictionary<int, UpgradeComponent> upgradeComponents)
    : ViewModel
{
    public event Action<UpgradeComponent>? Selected;

    public ItemIcons Icons { get; } = icons;

    public IReadOnlyDictionary<int, UpgradeComponent> UpgradeComponents { get; } = upgradeComponents;

    public void OnSelected(UpgradeComponent upgradeComponent)
    {
        Selected?.Invoke(upgradeComponent);
    }

    public IEnumerable<IGrouping<string, UpgradeComponent>> GetOptions()
    {
        var groupOrder = new Dictionary<string, int>
        {
            { "Runes", 1 },
            { "Sigils", 1 },
            { "Runes (PvP)", 2 },
            { "Sigils (PvP)", 2 },
            { "Infusions", 3 },
            { "Enrichments", 3 },
            { "Universal Upgrades", 4 },
            { "Uncategorized", 5 }
        };

        return from upgrade in UpgradeComponents.Values
               where FilterUpgradeSlot(item, upgrade)
               let rank = upgrade.Rarity.IsDefined()
                   ? upgrade.Rarity.ToEnum() switch
                   {
                       Rarity.Junk => 0,
                       Rarity.Basic => 1,
                       Rarity.Fine => 2,
                       Rarity.Masterwork => 3,
                       Rarity.Rare => 4,
                       Rarity.Exotic => 5,
                       Rarity.Ascended => 6,
                       Rarity.Legendary => 7,
                       _ => 99
                   }
                   : 99
               orderby rank, upgrade.Level, upgrade.Name
               group upgrade by upgrade switch
               {
                   Gem => "Universal Upgrades",
                   Rune when upgrade.GameTypes.All(
                       type => type.IsDefined() && type.ToEnum() is GameType.Pvp or GameType.PvpLobby) => "Runes (PvP)",
                   Sigil when upgrade.GameTypes.All(
                       type => type.IsDefined() && type.ToEnum() is GameType.Pvp or GameType.PvpLobby) => "Sigils (PvP)",
                   Rune => "Runes",
                   Sigil => "Sigils",
                   _ when upgrade.InfusionUpgradeFlags.Infusion => "Infusions",
                   _ when upgrade.InfusionUpgradeFlags.Enrichment => "Enrichments",
                   _ => "Uncategorized"
               }
            into grouped
               orderby groupOrder[grouped.Key]
               select grouped;
    }

    private bool FilterUpgradeSlot(Item item, UpgradeComponent component)
    {
        if (slotType == UpgradeSlotType.Infusion)
        {
            return component.InfusionUpgradeFlags.Infusion;
        }

        if (component.InfusionUpgradeFlags.Infusion)
        {
            return false;
        }

        if (slotType == UpgradeSlotType.Enrichment)
        {
            return component.InfusionUpgradeFlags.Enrichment;
        }

        if (component.InfusionUpgradeFlags.Enrichment)
        {
            return false;
        }

        if (component is Gem)
        {
            return true;
        }

        return item switch
        {
            Armor armor when armor.WeightClass == WeightClass.Light => component.UpgradeComponentFlags.LightArmor,
            Armor armor when armor.WeightClass == WeightClass.Medium => component.UpgradeComponentFlags.MediumArmor,
            Armor armor when armor.WeightClass == WeightClass.Heavy => component.UpgradeComponentFlags.HeavyArmor,
            Axe => component.UpgradeComponentFlags.Axe,
            Dagger => component.UpgradeComponentFlags.Dagger,
            Focus => component.UpgradeComponentFlags.Focus,
            Greatsword => component.UpgradeComponentFlags.Greatsword,
            Hammer => component.UpgradeComponentFlags.Hammer,
            HarpoonGun => component.UpgradeComponentFlags.HarpoonGun,
            Longbow => component.UpgradeComponentFlags.LongBow,
            Mace => component.UpgradeComponentFlags.Mace,
            Pistol => component.UpgradeComponentFlags.Pistol,
            Rifle => component.UpgradeComponentFlags.Rifle,
            Scepter => component.UpgradeComponentFlags.Scepter,
            Shield => component.UpgradeComponentFlags.Shield,
            Shortbow => component.UpgradeComponentFlags.ShortBow,
            Spear => component.UpgradeComponentFlags.Spear,
            Staff => component.UpgradeComponentFlags.Staff,
            Sword => component.UpgradeComponentFlags.Sword,
            Torch => component.UpgradeComponentFlags.Torch,
            Trident => component.UpgradeComponentFlags.Trident,
            Trinket => component.UpgradeComponentFlags.Trinket,
            Warhorn => component.UpgradeComponentFlags.Warhorn,
            _ => true
        };
    }
}

public sealed class UpgradeComponentsList : FlowPanel
{
    public UpgradeComponentsListViewModel ViewModel { get; }

    private readonly StandardButton _cancelButton;

    public UpgradeComponentsList(UpgradeComponentsListViewModel vm)
    {
        FlowDirection = ControlFlowDirection.SingleTopToBottom;
        HeightSizingMode = SizingMode.Fill;

        ViewModel = vm;
        _cancelButton = new StandardButton
        {
            Parent = this,
            Text = "Cancel"
        };

        _cancelButton.Click += (_, _) =>
        {
            Parent = null;
        };

        vm.Selected += _ =>
        {
            Parent = null;
        };

        Initialize();
    }

    protected override void OnResized(ResizedEventArgs e)
    {
        _cancelButton.Width = e.CurrentSize.X;
        Parent?.Invalidate();
    }

    private void Initialize()
    {
        foreach (var group in ViewModel.GetOptions())
        {
            var groupPanel = new Panel
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Title = group.Key,
                CanCollapse = true,
                Collapsed = true
            };

            var list = new ItemsList(ViewModel.Icons, ViewModel.UpgradeComponents)
            {
                Parent = groupPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                CanScroll = false
            };

            list.SetOptions(group);
            list.OptionClicked += OptionClicked;

            groupPanel.Resized += (sender, args) =>
            {
                if (list.Height >= 300)
                {
                    list.HeightSizingMode = SizingMode.Standard;
                    list.Height = 300;
                    list.CanScroll = true;
                }
                else
                {
                    list.HeightSizingMode = SizingMode.AutoSize;
                    list.CanScroll = false;
                }
            };
        }
    }

    private void OptionClicked(object sender, Item e)
    {
        ViewModel.OnSelected((UpgradeComponent)e);
    }
}