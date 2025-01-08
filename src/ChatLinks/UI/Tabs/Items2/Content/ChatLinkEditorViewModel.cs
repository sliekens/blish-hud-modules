using System.Diagnostics.CodeAnalysis;

using Blish_HUD.Content;

using GuildWars2.Chat;
using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common;
using SL.Common.Controls.Items.Services;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items2.Content;

public sealed class ChatLinkEditorViewModel : ViewModel
{
    private readonly ItemTooltipViewModelFactory _tooltipViewModelFactory;

    private readonly ItemIcons _icons;

    private readonly IClipBoard _clipboard;

    private int _quantity = 1;

    private ItemLink _chatLinkBuilder;

    public ChatLinkEditorViewModel(ItemTooltipViewModelFactory tooltipViewModelFactory,
        ItemIcons icons,
        IClipBoard clipboard,
        Item item)
    {
        _tooltipViewModelFactory = tooltipViewModelFactory;
        _icons = icons;
        _clipboard = clipboard;
        Item = item;
        ItemNameColor = ItemColors.Rarity(item.Rarity);
        ChatLink = item.ChatLink;
        Copy = new RelayCommand(DoCopy);
        MinQuantity = new RelayCommand(SetMinQuantity);
        MaxQuantity = new RelayCommand(SetMaxQuantity);
    }

    public Item Item { get; }

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

    public Color ItemNameColor { get; }

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

    public RelayCommand Copy { get; }

    public RelayCommand MinQuantity { get; set; }

    public RelayCommand MaxQuantity { get; set; }

    public ItemTooltipViewModel CreateTooltipViewModel()
    {
        return _tooltipViewModelFactory.Create(Item);
    }

    public AsyncTexture2D? GetIcon()
    {
        return _icons.GetIcon(Item);
    }

    private void DoCopy()
    {
        _clipboard.SetText(ChatLink);
    }

    private void SetMinQuantity()
    {
        Quantity = 1;
    }

    private void SetMaxQuantity()
    {
        Quantity = 250;
    }
}