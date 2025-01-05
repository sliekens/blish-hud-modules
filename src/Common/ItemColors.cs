using GuildWars2;
using GuildWars2.Items;

using Microsoft.Xna.Framework;

namespace SL.Common;

public static class ItemColors
{
    public static Color Rarity(Extensible<Rarity> rarity)
    {
        return rarity.ToEnum() switch
        {
            GuildWars2.Items.Rarity.Junk => MapColor(RarityColor.Junk),
            GuildWars2.Items.Rarity.Basic => MapColor(RarityColor.Basic),
            GuildWars2.Items.Rarity.Fine => MapColor(RarityColor.Fine),
            GuildWars2.Items.Rarity.Masterwork => MapColor(RarityColor.Masterwork),
            GuildWars2.Items.Rarity.Rare => MapColor(RarityColor.Rare),
            GuildWars2.Items.Rarity.Exotic => MapColor(RarityColor.Exotic),
            GuildWars2.Items.Rarity.Ascended => MapColor(RarityColor.Ascended),
            GuildWars2.Items.Rarity.Legendary => MapColor(RarityColor.Legendary),
            _ => Color.White
        };
    }

    private static Color MapColor(System.Drawing.Color color)
    {
        return new Color(color.R, color.G, color.B, color.A);
    }
}
