using GuildWars2.Hero.Equipment.Novelties;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Equipment.Novelties;

public sealed class NoveltyEntityTypeConfiguration : IEntityTypeConfiguration<Novelty>
{
    public void Configure(EntityTypeBuilder<Novelty> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.ToTable("Novelties");
        _ = builder.HasKey(novelty => novelty.Id);
        _ = builder.HasIndex(novelty => novelty.Name);
        _ = builder.Property(novelty => novelty.Slot)
            .HasConversion(new ExtensibleEnumConverter<NoveltyKind>());

        builder.Property(novelty => novelty.UnlockItemIds)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new CollectionComparer<int>());

#pragma warning disable CS0618 // Type or member is obsolete
        _ = builder.Ignore(novelty => novelty.IconHref);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
