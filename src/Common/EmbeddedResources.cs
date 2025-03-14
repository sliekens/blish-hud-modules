﻿using System.Reflection;

using Blish_HUD;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;


namespace SL.Common;

public static class EmbeddedResources
{
    public static Texture2D Texture(string name)
    {
        const string @namespace = "SL.Common.Resources.";
        using Blish_HUD.Graphics.GraphicsDeviceContext ctx = GameService.Graphics.LendGraphicsDeviceContext();
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(@namespace + name);
        return Texture2D.FromStream(ctx.GraphicsDevice, stream);
    }

    public static SoundEffect Sound(string name)
    {
        const string @namespace = "SL.Common.Resources.";
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(@namespace + name);
        return SoundEffect.FromStream(stream);
    }

}
