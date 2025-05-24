using GuildWars2.Hero.Equipment.Miniatures;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SL.ChatLinks.Storage.Models.Hero.Equipment.Miniatures;

public sealed class MiniatureEntityTypeConfiguration : IEntityTypeConfiguration<Miniature>
{
    public void Configure(EntityTypeBuilder<Miniature> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.ToTable("Miniatures");
        _ = builder.HasKey(mini => mini.Id);
        _ = builder.HasIndex(mini => mini.Name);
        _ = builder.HasIndex(mini => mini.ItemId);

#pragma warning disable CS0618 // Type or member is obsolete
        _ = builder.Ignore(mini => mini.IconHref);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
