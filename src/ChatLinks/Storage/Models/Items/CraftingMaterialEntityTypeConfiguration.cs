using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public sealed class CraftingMaterialEntityTypeConfiguration(
    ValueComparer<IReadOnlyCollection<InfusionSlotUpgradePath>> upgradePathComparer
) : IEntityTypeConfiguration<CraftingMaterial>
{
    public void Configure(EntityTypeBuilder<CraftingMaterial> builder)
    {
        builder.Property(craftingMaterial => craftingMaterial.UpgradesInto)
            .HasColumnName("UpgradesInto")
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(upgradePathComparer);
    }
}
