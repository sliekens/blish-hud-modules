using GuildWars2;
using GuildWars2.Items;

using Microsoft.Xna.Framework;

namespace ChatLinksModule.UI.Tabs.Items;

public static class ItemColors
{
    public static Color Rarity(Extensible<Rarity> rarity)
    {
        return rarity.ToEnum() switch
        {
            GuildWars2.Items.Rarity.Junk => new Color(0x99, 0x99, 0x99),
            GuildWars2.Items.Rarity.Basic => Color.White,
            GuildWars2.Items.Rarity.Fine => new Color(0x44, 0x99, 0xEE),
            GuildWars2.Items.Rarity.Masterwork => new Color(0x22, 0xBB, 0x11),
            GuildWars2.Items.Rarity.Rare => new Color(0xEE, 0xDD, 0x22),
            GuildWars2.Items.Rarity.Exotic => new Color(0xEE, 0x99, 0x00),
            GuildWars2.Items.Rarity.Ascended => new Color(0xEE, 0x33, 0x88),
            _ => Color.White
        };
    }
}