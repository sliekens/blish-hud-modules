using GuildWars2.Hero.Equipment.Gliders;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models;

public sealed class GliderSkinEntityTypeConfiguration : IEntityTypeConfiguration<GliderSkin>
{
    public void Configure(EntityTypeBuilder<GliderSkin> builder)
    {
        builder.ToTable("Gliders");
        builder.HasKey(glider => glider.Id);
        builder.HasIndex(glider => glider.Name);
        builder.HasIndex(glider => glider.Order);

        builder.Property(finisher => finisher.UnlockItemIds)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new CollectionComparer<int>());

        builder.Property(finisher => finisher.DefaultDyeColorIds)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new ListComparer<int>());
    }
}