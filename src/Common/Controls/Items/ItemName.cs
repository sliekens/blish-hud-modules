using Blish_HUD.Controls;

using GuildWars2.Items;

namespace SL.Common.Controls.Items;

public class ItemName : Label
{
    private readonly Item _item;

    private int _quantity;

    public ItemName(Item item)
    {
        Text = item.Name;
        TextColor = ItemColors.Rarity(item.Rarity);

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
