using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public sealed class RuneEntityTypeConfiguration : IEntityTypeConfiguration<Rune>
{
    public void Configure(EntityTypeBuilder<Rune> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.Property(rune => rune.Bonuses)
            .HasJsonValueConversion();
    }
}
