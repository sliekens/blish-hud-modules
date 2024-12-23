using Blish_HUD.Controls;
using Blish_HUD.Input;

using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.Common.Controls.Items;
using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public class UpgradeComponentsList : FlowPanel
{
    public event EventHandler<UpgradeComponentSelectedArgs>? UpgradeComponentSelected;

    private readonly UpgradeSlotType _slotType;

    private readonly List<UpgradeComponent> _upgrades;

    private readonly Dictionary<int, UpgradeComponent> _upgradesDictionary;

    private readonly ItemIcons _icons;

    private readonly Item _target;

    private bool _initialized;

    public UpgradeComponentsList(UpgradeSlotType slotType, IEnumerable<UpgradeComponent> upgrades, ItemIcons icons, Item target)
    {
        FlowDirection = ControlFlowDirection.SingleTopToBottom;
        _slotType = slotType;
        _upgrades = upgrades.ToList();
        _upgradesDictionary = _upgrades.ToDictionary(static upgrade => upgrade.Id);
        _icons = icons;
        _target = target;
    }

    public override void UpdateContainer(GameTime gameTime)
    {
        if (!_initialized)
        {
            Initialize();
            _initialized = true;
        }

        base.UpdateContainer(gameTime);
    }

    private void Initialize()
    {
        var cancel = new StandardButton
        {
            Parent = this,
            Width = Width,
            Text = "Cancel"
        };

        cancel.Click += (_, _) =>
        {
            Hide();
            Parent?.Invalidate();
        };

        var groups = _upgrades
            .Where(FilterUpgradeSlot)
            .OrderBy(upgrade => upgrade.Level)
            .ThenBy(upgrade => upgrade.Name)
            .GroupBy(u => u switch
            {
                Gem => ("Universal Upgrades", 1),
                Rune when u.GameTypes.All(type => type.IsDefined() && type.ToEnum() is GameType.Pvp or GameType.PvpLobby) => ("Runes (PvP)", 3),
                Sigil when u.GameTypes.All(type => type.IsDefined() && type.ToEnum() is GameType.Pvp or GameType.PvpLobby) => ("Sigils (PvP)", 5),
                Rune => ("Runes", 2),
                Sigil => ("Sigils", 4),
                _ when u.InfusionUpgradeFlags.Infusion => ("Infusions", 6),
                _ when u.InfusionUpgradeFlags.Enrichment => ("Enrichments", 7),
                _ => ("Other", 8)
            });

        foreach (var group in groups.OrderBy(g => g.Key.Item2))
        {
            var groupPanel = new Panel
            {
                Parent = this,
                Width = Width,
                HeightSizingMode = SizingMode.AutoSize,
                Title = group.Key.Item1,
                CanCollapse = true,
                Collapsed = true
            };

            var list = new ItemsList(_icons, _upgradesDictionary)
            {
                Parent = groupPanel,
                Width = Width,
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

    private bool FilterUpgradeSlot(UpgradeComponent component)
    {
        if ((_slotType == UpgradeSlotType.Infusion) ^ component.InfusionUpgradeFlags.Infusion)
        {
            return false;
        }

        if ((_slotType == UpgradeSlotType.Enrichment) ^ component.InfusionUpgradeFlags.Enrichment)
        {
            return false;
        }

        if (_slotType == UpgradeSlotType.Default && component is Gem)
        {
            return true;
        }

        return _target switch
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

    private void OptionClicked(object sender, Item e)
    {
        UpgradeComponentSelected?.Invoke(this, new UpgradeComponentSelectedArgs((UpgradeComponent)e));
    }
}