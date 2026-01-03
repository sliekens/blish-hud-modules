using Blish_HUD;
using Blish_HUD.Content;

namespace SL.Common;

public static class ContentServiceExtensions
{
    public static AsyncTexture2D GetRenderServiceTexture(this ContentService instance, Uri url)
    {
        ThrowHelper.ThrowIfNull(instance);
        ThrowHelper.ThrowIfNull(url);

#pragma warning disable CA2234 // Pass system uri objects instead of strings
        return instance.GetRenderServiceTexture(url.ToString());
#pragma warning restore CA2234 // Pass system uri objects instead of strings
    }
}
