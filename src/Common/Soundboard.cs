using Blish_HUD;

using Microsoft.Xna.Framework.Audio;

namespace SL.Common;

public static class Soundboard
{
    private static SoundEffect ClickSound { get; } = EmbeddedResources.Sound("click.wav");

    public static void Click()
    {
        ClickSound.Play(GameService.GameIntegration.Audio.Volume, 0, 0);
    }
}
