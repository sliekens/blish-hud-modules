using GuildWars2.Items;

namespace SL.ChatLinks.UI.Tabs.Items;

internal static class ItemExtensions
{
    public static Uri? IconUrl(this Item item)
    {
        ThrowHelper.ThrowIfNull(item);
        return !string.IsNullOrEmpty(item.IconHref)
            ? new Uri(item.IconHref!)
            : null;
    }

    public static Uri? IconUrl(this Effect effect)
    {
        ThrowHelper.ThrowIfNull(effect);
        return !string.IsNullOrEmpty(effect.IconHref)
            ? new Uri(effect.IconHref!)
            : null;
    }

}
