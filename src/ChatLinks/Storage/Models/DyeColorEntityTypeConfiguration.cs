using System.Drawing;

using GuildWars2.Hero.Equipment.Dyes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models;

public sealed class DyeColorEntityTypeConfiguration : IEntityTypeConfiguration<DyeColor>
{
    public void Configure(EntityTypeBuilder<DyeColor> builder)
    {
        builder.ToTable("Colors");
        builder.HasKey(color => color.Id);
        builder.HasIndex(color => color.Name);
        builder.HasIndex(color => color.ItemId);
        builder.HasIndex(color => color.Hue);
        builder.HasIndex(color => color.Material);
        builder.HasIndex(color => color.Set);
        builder.Property(color => color.BaseRgb).HasConversion(
            color => color.ToArgb(),
            argb => Color.FromArgb(argb));
        builder.Property(color => color.Cloth).HasJsonValueConversion();
        builder.Property(color => color.Leather).HasJsonValueConversion();
        builder.Property(color => color.Metal).HasJsonValueConversion();
        builder.Property(color => color.Fur).HasJsonValueConversion();
        builder.Property(color => color.Hue).HasConversion(new ExtensibleEnumConverter<Hue>());
        builder.Property(color => color.Material).HasConversion(new ExtensibleEnumConverter<Material>());
        builder.Property(color => color.Set).HasConversion(new ExtensibleEnumConverter<ColorSet>());
    }
}