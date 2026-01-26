namespace SL.ChatLinks.Storage.Models.Hero.Equipment.Skiffs;

public sealed record SkiffSkinUnlock
{
    public required int SkiffSkinId { get; init; }
    public required int ItemId { get; init; }
}
