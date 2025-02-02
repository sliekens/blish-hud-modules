using GuildWars2.Hero.Equipment.Novelties;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models;

public sealed class NoveltyEntityTypeConfiguration : IEntityTypeConfiguration<Novelty>
{
    public void Configure(EntityTypeBuilder<Novelty> builder)
    {
        builder.ToTable("Novelties");
        builder.HasKey(novelty => novelty.Id);
        builder.HasIndex(novelty => novelty.Name);
        builder.Property(novelty => novelty.Slot)
            .HasConversion(new ExtensibleEnumConverter<NoveltyKind>());

        builder.Property(novelty => novelty.UnlockItemIds)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new CollectionComparer<int>());
    }
}