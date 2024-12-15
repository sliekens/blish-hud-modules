using Blish_HUD.Controls;

using GuildWars2.Items;

namespace SL.Common.Controls.Items;

public class ItemName : Label
{
    private readonly Item _item;

    private int _quantity;

    public ItemName(Item item)
    {
        TextColor = ItemColors.Rarity(item.Rarity);
        Text = item.Name;
        if (item.GameTypes.All(type => type.IsDefined() && type.ToEnum() is GameType.Pvp or GameType.PvpLobby))
        {
            Text += " (PvP)";
        }

        _item = item;
    }

    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            Text = _quantity == 1 ? _item.Name : $"{_quantity} {_item.Name}";
        }
    }
}
