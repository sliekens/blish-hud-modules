using Blish_HUD.Controls;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

namespace ChatLinksModule.UI.Tabs.Items.Controls;

public class ItemsList : FlowPanel
{
    public ItemsList()
    {
        Size = new Point(450, 500);
        ShowTint = true;
        ShowBorder = true;
        CanScroll = true;
    }

    public void SetOptions(IEnumerable<Item> items)
    {
        while (!Children.IsEmpty)
        {
            Children[0].Dispose();
        }

        foreach (Item item in items)
        {
            _ = new ItemChoice(item) { Parent = this };
        }
    }
}