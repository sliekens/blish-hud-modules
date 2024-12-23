using Blish_HUD;
using System.Diagnostics;
using System.Net;
using System.Text.Encodings.Web;

using Blish_HUD.Controls;

using GuildWars2.Items;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public class ItemContextMenu : ContextMenuStrip
{
    public ItemContextMenu(Item item)
    {
        ContextMenuStripItem copyName = AddMenuItem("Copy Name");
        ContextMenuStripItem wiki = AddMenuItem("Wiki");
        ContextMenuStripItem api = AddMenuItem("API");

        copyName.Click += async (_, _) => await ClipboardUtil.WindowsClipboardService.SetTextAsync(item.Name);

        api.Click += (_, _) => Process.Start($"https://api.guildwars2.com/v2/items/{item.Id}?v=latest");

        wiki.Click += (_, _) => Process.Start($"https://wiki.guildwars2.com/wiki/?search={WebUtility.UrlEncode(item.ChatLink)}");
    }

}