using Blish_HUD.Graphics.UI;

using GuildWars2.Items;

namespace SL.ChatLinks.UI.Tabs.Items;

public interface IItemsTabView : IView
{
    void SetSearchLoading(bool loading);

    void AddOption(Item item);

    void SetOptions(IEnumerable<Item> items);

    void ClearOptions();

    void Select(Item item);
}