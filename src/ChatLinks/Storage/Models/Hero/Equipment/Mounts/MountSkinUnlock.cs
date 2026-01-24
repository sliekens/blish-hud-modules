namespace SL.ChatLinks.Storage.Models.Hero.Equipment.Mounts;

public sealed record MountSkinUnlock
{
    public required int MountSkinId { get; init; }
    public required int ItemId { get; init; }
}
