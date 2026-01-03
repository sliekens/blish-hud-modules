using GuildWars2.Hero.Equipment.Gliders;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Equipment.Gliders;

public sealed class GliderSkinEntityTypeConfiguration : IEntityTypeConfiguration<GliderSkin>
{
    public void Configure(EntityTypeBuilder<GliderSkin> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.ToTable("Gliders");
        _ = builder.HasKey(glider => glider.Id);
        _ = builder.HasIndex(glider => glider.Name);
        _ = builder.HasIndex(glider => glider.Order);

        builder.Property(glider => glider.UnlockItemIds)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new CollectionComparer<int>());

        builder.Property(glider => glider.DefaultDyeColorIds)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new ListComparer<int>());
    }
}
