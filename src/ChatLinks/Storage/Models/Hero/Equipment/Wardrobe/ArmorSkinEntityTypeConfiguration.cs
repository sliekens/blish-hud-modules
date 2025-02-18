using GuildWars2.Hero;
using GuildWars2.Hero.Equipment.Wardrobe;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Equipment.Wardrobe;

public sealed class ArmorSkinEntityTypeConfiguration : IEntityTypeConfiguration<ArmorSkin>
{
    public void Configure(EntityTypeBuilder<ArmorSkin> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.Property(skin => skin.WeightClass).HasConversion(new ExtensibleEnumConverter<WeightClass>());
        _ = builder.Property(skin => skin.DyeSlots)
            .HasJsonValueConversion();
    }
}
