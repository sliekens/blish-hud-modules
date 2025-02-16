using Microsoft.Xna.Framework.Audio;

namespace SL.Common;

public static class Soundboard
{
    public static SoundEffect Click { get; } = EmbeddedResources.Sound("click.wav");
}
