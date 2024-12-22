using Blish_HUD;
using Blish_HUD.Content;

using Microsoft.Xna.Framework.Graphics;

namespace SL.Common;

public static class AsyncTexture2DExtensions
{
    public static AsyncTexture2D Clone(this AsyncTexture2D instance)
    {
        if (instance.HasSwapped)
        {
            return instance.Texture;
        }

        AsyncTexture2D clone = new();
        instance.TextureSwapped += OnTextureSwapped;
        return clone;

        void OnTextureSwapped(object sender, ValueChangedEventArgs<Texture2D> e)
        {
            clone.SwapTexture(e.NewValue);
            instance.TextureSwapped -= OnTextureSwapped;
        }
    }

}