using GuildWars2.Hero.Equipment.Miniatures;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SL.ChatLinks.Storage.Models.Hero.Equipment.Miniatures;

public sealed class MiniatureEntityTypeConfiguration : IEntityTypeConfiguration<Miniature>
{
    public void Configure(EntityTypeBuilder<Miniature> builder)
    {
        builder.ToTable("Miniatures");
        builder.HasKey(mini => mini.Id);
        builder.HasIndex(mini => mini.Name);
        builder.HasIndex(mini => mini.ItemId);
    }
}
