using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

using ChatLinksModule.Storage;

using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;

using Container = Blish_HUD.Controls.Container;

namespace ChatLinksModule.UI.Tabs.Items;

public class ItemsView(ChatLinksContext db) : View
{
    private TextBox _searchBox;

    protected override void Build(Container buildPanel)
    {
        _searchBox = new TextBox { Parent = buildPanel };

        _searchBox.TextChanged += SearchInput;
    }

    private async void SearchInput(object sender, EventArgs e)
    {
        ScreenNotification.ShowNotification(_searchBox.Text);
        string search = _searchBox.Text.Trim();
        if (search.Length < 3)
        {
            return;
        }

        List<Item> items = await db.Items.AsQueryable()
            .Where(item => item.Name.Contains(search) || item.Description.Contains(search))
            .ToListAsync();
    }
}