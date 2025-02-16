using GuildWars2.Hero.Equipment.Wardrobe;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Equipment.Wardrobe;

public sealed class WeaponSkinEntityTypeConfiguration : IEntityTypeConfiguration<WeaponSkin>
{
    public void Configure(EntityTypeBuilder<WeaponSkin> builder)
    {
        builder.Property(skin => skin.DamageType).HasConversion(new ExtensibleEnumConverter<DamageType>());
    }
}
