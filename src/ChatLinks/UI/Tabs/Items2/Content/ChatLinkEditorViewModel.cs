using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

using Blish_HUD.Content;

using GuildWars2.Chat;
using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items2.Content.Upgrades;
using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common;
using SL.Common.Controls.Items.Services;
using SL.Common.Controls.Items.Upgrades;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items2.Content;

public sealed class ChatLinkEditorViewModel(
    ItemTooltipViewModelFactory tooltipViewModelFactory,
    UpgradeEditorViewModelFactory upgradeEditorViewModelFactory,
    ItemIcons icons,
    IClipBoard clipboard,
    Item item
) : ViewModel
{
    private int _quantity = 1;

    private ItemLink _chatLinkBuilder = new() { ItemId = item.Id };

    public Item Item { get; } = item;

    public string ItemName
    {
        get
        {
            if (Quantity <= 1)
            {
                return Item.Name;
            }

            return $"{Quantity} {Item.Name}";
        }
    }

    public Color ItemNameColor { get; } = ItemColors.Rarity(item.Rarity);

    public int Quantity
    {
        get => _quantity;
        set
        {
            OnPropertyChanging(nameof(ItemName));
            OnPropertyChanging(nameof(ChatLink));
            SetField(ref _quantity, value);
            _chatLinkBuilder = _chatLinkBuilder with { Count = value };
            OnPropertyChanged(nameof(ItemName));
            OnPropertyChanged(nameof(ChatLink));
        }
    }

    public string ChatLink
    {
        get => _chatLinkBuilder.ToString();
        [MemberNotNull(nameof(_chatLinkBuilder))]
        set
        {
            OnPropertyChanging();
            _chatLinkBuilder = ItemLink.Parse(value);
            OnPropertyChanged();
        }
    }

    public ICommand CopyCommand => new RelayCommand(OnCopy);

    public ICommand MinQuantityCommand => new RelayCommand(OnMinQuantity);

    public ICommand MaxQuantityCommand => new RelayCommand(OnMaxQuantity);

    public ItemTooltipViewModel CreateTooltipViewModel()
    {
        return tooltipViewModelFactory.Create(Item);
    }

    public AsyncTexture2D? GetIcon()
    {
        return icons.GetIcon(Item);
    }

    public IEnumerable<UpgradeEditorViewModel> UpgradeSlots()
    {
        if (Item is not IUpgradable upgradable)
        {
            yield break;
        }

        foreach (var defaultUpgradeComponentId in upgradable.UpgradeSlots)
        {
            yield return upgradeEditorViewModelFactory.Create(Item, UpgradeSlotType.Default, defaultUpgradeComponentId);
        }

        foreach (var infusionSlot in upgradable.InfusionSlots)
        {
            yield return upgradeEditorViewModelFactory.Create(
                Item,
                infusionSlot.Flags switch
                {
                    { Enrichment: true } => UpgradeSlotType.Enrichment,
                    { Infusion: true } => UpgradeSlotType.Infusion,
                    _ => UpgradeSlotType.Default
                },
                infusionSlot.ItemId
            );
        }
    }

    private void OnCopy()
    {
        clipboard.SetText(ChatLink);
    }

    private void OnMinQuantity()
    {
        Quantity = 1;
    }

    private void OnMaxQuantity()
    {
        Quantity = 250;
    }
}