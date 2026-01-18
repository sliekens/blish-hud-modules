using GuildWars2.Hero.Equipment.Novelties;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

        _ = builder.Property(novelty => novelty.UnlockItemIds)
            .HasJsonValueConversion();
    }
}
