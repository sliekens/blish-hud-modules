using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public sealed class CraftingMaterialEntityTypeConfiguration : IEntityTypeConfiguration<CraftingMaterial>
{
    public void Configure(EntityTypeBuilder<CraftingMaterial> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.Property(craftingMaterial => craftingMaterial.UpgradesInto)
            .HasColumnName("UpgradesInto")
            .HasJsonValueConversion();
    }
}
