namespace SL.ChatLinks.UI.Tabs.Items;

public sealed record ItemCategoryMenuItem
{
    public string? Id { get; init; }

    public required string Label { get; init; }

    public List<ItemCategoryMenuItem> Subcategories { get; init; } = [];

    public bool CanSelect => Id is not null && Subcategories.Count == 0;
}
