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
        builder.ToTable("Outfits");
        builder.HasKey(outfit => outfit.Id);
        builder.HasIndex(outfit => outfit.Name);

        builder.Property(outfit => outfit.UnlockItemIds)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new CollectionComparer<int>());
    }
}