using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;

using ChatLinksModule.Storage;

using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;

using Container = Blish_HUD.Controls.Container;

namespace ChatLinksModule.UI.Tabs.Items;

public class ItemsView(ChatLinksContext db, ILogger<ItemsView> logger) : View
{
    private readonly SemaphoreSlim _searchLock = new(1, 1);

    private FlowPanel _itemsPanel;

    private Container _root;

    private TextBox _searchBox;

    private ItemWidget _selectedItem;

    protected override void Build(Container buildPanel)
    {
        _root = buildPanel;
        _searchBox = new TextBox { Parent = buildPanel, Width = 450 };
        _itemsPanel = new FlowPanel
        {
            Parent = buildPanel,
            Size = new Point(450, 500),
            Top = _searchBox.Bottom,
            ShowTint = true,
            ShowBorder = true,
            CanScroll = true
        };

        _searchBox.TextChanged += SearchInput;
        _itemsPanel.Click += ItemClicked;
    }

    private void ItemClicked(object sender, MouseEventArgs e)
    {
        ItemCard clickedItem = _itemsPanel.Children.OfType<ItemCard>()
            .SingleOrDefault(entry => entry.AbsoluteBounds.Contains(e.MousePosition));
        ShowWidget(clickedItem?.Item);
    }

    private void ShowWidget(Item? item)
    {
        _selectedItem?.Dispose();
        _selectedItem = item is not null ? new ItemWidget(item) { Parent = _root, Left = _itemsPanel.Right } : null;
    }

    private async void SearchInput(object sender, EventArgs e)
    {
        try
        {
            // Avoid blocking UI
            await Task.Yield();

            // Ensure exclusive access to the DbContext (not thread-safe)
            await _searchLock.WaitAsync();

            try
            {
                List<Item> results = [];
                string search = _searchBox.Text.ToLowerInvariant().Trim();
                if (search.Length > 3)
                {
                    results = await db.Items.AsQueryable()
                        .Where(i => i.Name.ToLower().Contains(search))
                        .Take(100)
                        .ToListAsync();
                }

                UpdateSearchResults(results);
            }
            finally
            {
                _searchLock.Release();
            }
        }
        catch (Exception reason)
        {
            logger.LogError(reason, "Failed to search for items");
            ScreenNotification.ShowNotification("Something went wrong", ScreenNotification.NotificationType.Red);
        }
    }

    private void UpdateSearchResults(IEnumerable<Item> items)
    {
        _itemsPanel.ClearChildren();
        foreach (Item item in items)
        {
            _ = new ItemCard(item) { Parent = _itemsPanel };
        }
    }

    protected override void Unload()
    {
        _searchBox.TextChanged -= SearchInput;
        _searchBox.Dispose();
        _itemsPanel.Dispose();
        base.Unload();
    }
}