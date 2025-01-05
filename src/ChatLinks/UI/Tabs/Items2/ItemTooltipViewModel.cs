using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using SL.Common;
using SL.Common.Controls.Items.Services;

namespace SL.ChatLinks.UI.Tabs.Items2;

public sealed class ItemTooltipViewModel(ItemIcons icons, Customizer customizer, Item item) : ViewModel
{
    public Item Item { get; } = item;

    public IReadOnlyDictionary<int, UpgradeComponent> Upgrades => customizer.UpgradeComponents;

    public string AttributeName(Extensible<AttributeName> stat)
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

    public ItemUpgradeBuilder UpgradeBuilder(ItemFlags flags)
    {
        return new ItemUpgradeBuilder(flags, icons, customizer);
    }
}