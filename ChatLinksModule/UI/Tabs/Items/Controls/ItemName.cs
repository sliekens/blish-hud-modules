using Blish_HUD.Controls;

using GuildWars2.Items;

namespace ChatLinksModule.UI.Tabs.Items.Controls;

public class ItemName : Label
{
    private readonly Item _item;

    private int _quantity;

    public ItemName(Item item)
    {
        _item = item;
        WrapText = true;
        Width = 240;
        Height = 50;
        VerticalAlignment = VerticalAlignment.Middle;
        Text = item.Name;
        TextColor = ItemColors.Rarity(item.Rarity);
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