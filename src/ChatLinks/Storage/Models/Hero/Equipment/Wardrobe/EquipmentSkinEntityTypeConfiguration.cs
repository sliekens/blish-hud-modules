using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Hero.Equipment.Wardrobe;
using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Equipment.Wardrobe;

public sealed class EquipmentSkinEntityTypeConfiguration : IEntityTypeConfiguration<EquipmentSkin>
{
    public void Configure(EntityTypeBuilder<EquipmentSkin> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.ToTable("Skins");
        _ = builder.HasKey(skin => skin.Id);
        _ = builder.HasIndex(skin => skin.Name);
        _ = builder.Property(skin => skin.Flags).HasJsonValueConversion();
        builder.Property(skin => skin.Races).HasJsonValueConversion()
            .Metadata.SetValueComparer(new ListComparer<Extensible<RaceName>>());
        _ = builder.Property(skin => skin.Rarity).HasConversion(new ExtensibleEnumConverter<Rarity>());

#pragma warning disable CS0618 // Type or member is obsolete
        _ = builder.Ignore(skin => skin.IconHref);
#pragma warning restore CS0618 // Type or member is obsolete

        DiscriminatorBuilder<string> discriminatorBuilder = builder.HasDiscriminator<string>("Type");
        _ = discriminatorBuilder.HasValue<EquipmentSkin>("skin");
        _ = discriminatorBuilder.HasValue<ArmorSkin>("armor")
            .HasValue<BootsSkin>("boots")
            .HasValue<CoatSkin>("coat")
            .HasValue<GlovesSkin>("gloves")
            .HasValue<HelmAquaticSkin>("helm_aquatic")
            .HasValue<HelmSkin>("helm")
            .HasValue<LeggingsSkin>("leggings")
            .HasValue<ShouldersSkin>("shoulders")
            ;
        _ = discriminatorBuilder.HasValue<BackpackSkin>("back");
        _ = discriminatorBuilder.HasValue<GatheringToolSkin>("gathering_tool")
            .HasValue<FishingToolSkin>("fishing_tool")
            .HasValue<ForagingToolSkin>("foraging_tool")
            .HasValue<LoggingToolSkin>("logging_tool")
            .HasValue<MiningToolSkin>("mining_tool")
            ;
        _ = discriminatorBuilder.HasValue<WeaponSkin>("weapon")
            .HasValue<AxeSkin>("axe")
            .HasValue<DaggerSkin>("dagger")
            .HasValue<FocusSkin>("focus")
            .HasValue<GreatswordSkin>("greatsword")
            .HasValue<HammerSkin>("hammer")
            .HasValue<HarpoonGunSkin>("harpoon_gun")
            .HasValue<LargeBundleSkin>("large_bundle")
            .HasValue<LongbowSkin>("longbow")
            .HasValue<MaceSkin>("mace")
            .HasValue<PistolSkin>("pistol")
            .HasValue<RifleSkin>("rifle")
            .HasValue<ScepterSkin>("scepter")
            .HasValue<ShieldSkin>("shield")
            .HasValue<ShortbowSkin>("shortbow")
            .HasValue<SmallBundleSkin>("small_bundle")
            .HasValue<SpearSkin>("spear")
            .HasValue<StaffSkin>("staff")
            .HasValue<SwordSkin>("sword")
            .HasValue<TorchSkin>("torch")
            .HasValue<ToySkin>("toy")
            .HasValue<ToyTwoHandedSkin>("toy_two_handed")
            .HasValue<TridentSkin>("trident")
            .HasValue<WarhornSkin>("warhorn")
            ;
    }
}
