using Blish_HUD.Content;

using GuildWars2.Items;

using SL.Common.Controls.Items.Services;

namespace SL.ChatLinks.UI.Tabs.Items2;

public sealed class ItemsListEntryViewModel(ItemIcons icons, Item item)
{
    public Item Item { get; } = item ?? throw new ArgumentNullException(nameof(item));

    public AsyncTexture2D? GetIcon()
    {
        return icons.GetIcon(item);
    }
}