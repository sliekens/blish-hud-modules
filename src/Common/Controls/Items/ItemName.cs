using Blish_HUD.Controls;

using GuildWars2.Items;

namespace SL.Common.Controls.Items;

public class ItemName : Label
{
    private readonly Item _item;

    private readonly string _baseName;

    private UpgradeComponent? _suffixItem;

    private int _quantity;

    public ItemName(Item item, IDictionary<int, UpgradeComponent> upgrades)
    {
        TextColor = ItemColors.Rarity(item.Rarity);
        _item = item;
        _baseName ??= item.NameWithoutSuffix(upgrades);

        UpdateText();
    }

    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            UpdateText();
        }
    }

    public UpgradeComponent? SuffixItem
    {
        get => _suffixItem;
        set
        {
            _suffixItem = value;
            UpdateText();
        }
    }

    private void UpdateText()
    {
        Text = _suffixItem is not null && !_item.Flags.HideSuffix
            ? $"{_baseName} {_suffixItem.SuffixName}"
            : _item.Name;

        if (_quantity > 1)
        {
            Text = $"{_quantity} {Text}";
        }

        if (_item.GameTypes.All(type => type.IsDefined() && type.ToEnum() is GameType.Pvp or GameType.PvpLobby))
        {
            Text += " (PvP)";
        }
    }
}
