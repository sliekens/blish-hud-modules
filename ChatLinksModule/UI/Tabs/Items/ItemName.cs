using Blish_HUD.Controls;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

namespace ChatLinksModule.UI.Tabs.Items;

public class ItemName : Label
{
    public ItemName(Item item)
    {
        WrapText = true;
        Width = 240;
        Height = 50;
        VerticalAlignment = VerticalAlignment.Middle;
        Text = item.Name;
        TextColor = item.Rarity.ToEnum() switch
        {
            Rarity.Junk => new Color(0x99, 0x99, 0x99),
            Rarity.Basic => Color.White,
            Rarity.Fine => new Color(0x44, 0x99, 0xEE),
            Rarity.Masterwork => new Color(0x22, 0xBB, 0x11),
            Rarity.Rare => new Color(0xEE, 0xDD, 0x22),
            Rarity.Exotic => new Color(0xEE, 0x99, 0x00),
            Rarity.Ascended => new Color(0xEE, 0x33, 0x88),
            _ => Color.White
        };
    }
}