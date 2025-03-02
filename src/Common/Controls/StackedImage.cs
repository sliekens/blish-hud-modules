using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SL.Common.Controls;

public sealed class StackedImage : Control
{
    public IList<(AsyncTexture2D, Color)> Textures { get; } = [];

    protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
    {
        foreach ((AsyncTexture2D texture, Color tint) in Textures)
        {
            spriteBatch.DrawOnCtrl(this, texture, bounds, texture.Bounds, tint, 0f, Vector2.Zero, SpriteEffects.None);
        }
    }
}
