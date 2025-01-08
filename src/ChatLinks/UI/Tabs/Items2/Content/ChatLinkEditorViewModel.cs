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

    public ChatLinkEditorViewModel(ItemTooltipViewModelFactory tooltipViewModelFactory,
        ItemIcons icons,
        IClipBoard clipboard,
        Item item)
    {
        _tooltipViewModelFactory = tooltipViewModelFactory;
        _icons = icons;
        _clipboard = clipboard;
        Item = item;
        ItemName = item.Name;
        ItemNameColor = ItemColors.Rarity(item.Rarity);
        ChatLink = item.GetChatLink();
        Copy = new RelayCommand(DoCopy);
    }

    public Item Item { get; }

    public string ItemName { get; }

    public Color ItemNameColor { get; }

    public ItemLink ChatLink { get; private set; }

    public RelayCommand Copy { get; }

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
        _clipboard.SetText(ChatLink.ToString());
    }
}