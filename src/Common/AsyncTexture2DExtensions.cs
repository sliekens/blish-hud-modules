using Blish_HUD;
using Blish_HUD.Content;

using Microsoft.Xna.Framework.Graphics;

namespace SL.Common;

public static class AsyncTexture2DExtensions
{
    public static AsyncTexture2D Duplicate(this AsyncTexture2D instance)
    {
        if (instance.HasSwapped)
        {
            return instance.Texture.Duplicate();
        }

        AsyncTexture2D duplicate = new();
        instance.TextureSwapped += OnTextureSwapped;
        return duplicate;

        void OnTextureSwapped(object sender, ValueChangedEventArgs<Texture2D> e)
        {
            duplicate.SwapTexture(e.NewValue.Duplicate());
            instance.TextureSwapped -= OnTextureSwapped;
        }
    }

}
