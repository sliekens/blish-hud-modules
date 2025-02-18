using System.Drawing;

using GuildWars2.Hero.Equipment.Dyes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Equipment.Dyes;

public sealed class DyeColorEntityTypeConfiguration : IEntityTypeConfiguration<DyeColor>
{
    public void Configure(EntityTypeBuilder<DyeColor> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.ToTable("Colors");
        _ = builder.HasKey(color => color.Id);
        _ = builder.HasIndex(color => color.Name);
        _ = builder.HasIndex(color => color.ItemId);
        _ = builder.HasIndex(color => color.Hue);
        _ = builder.HasIndex(color => color.Material);
        _ = builder.HasIndex(color => color.Set);
        _ = builder.Property(color => color.BaseRgb).HasConversion(
            color => color.ToArgb(),
            argb => Color.FromArgb(argb));
        _ = builder.Property(color => color.Cloth).HasJsonValueConversion();
        _ = builder.Property(color => color.Leather).HasJsonValueConversion();
        _ = builder.Property(color => color.Metal).HasJsonValueConversion();
        _ = builder.Property(color => color.Fur).HasJsonValueConversion();
        _ = builder.Property(color => color.Hue).HasConversion(new ExtensibleEnumConverter<Hue>());
        _ = builder.Property(color => color.Material).HasConversion(new ExtensibleEnumConverter<Material>());
        _ = builder.Property(color => color.Set).HasConversion(new ExtensibleEnumConverter<ColorSet>());
    }
}
