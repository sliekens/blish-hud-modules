using GuildWars2.Hero.Equipment.Outfits;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Equipment.Outfits;

public sealed class OutfitEntityTypeConfiguration : IEntityTypeConfiguration<Outfit>
{
    public void Configure(EntityTypeBuilder<Outfit> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.ToTable("Outfits");
        _ = builder.HasKey(outfit => outfit.Id);
        _ = builder.HasIndex(outfit => outfit.Name);

#pragma warning disable CS0618 // Type or member is obsolete
        _ = builder.Ignore(outfit => outfit.IconHref);
#pragma warning restore CS0618 // Type or member is obsolete

        builder.Property(outfit => outfit.UnlockItemIds)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new CollectionComparer<int>());
    }
}
