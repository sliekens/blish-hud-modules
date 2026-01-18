using GuildWars2.Hero.Equipment.Finishers;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Equipment.Finishers;

public sealed class FinisherEntityTypeConfiguration : IEntityTypeConfiguration<Finisher>
{
    public void Configure(EntityTypeBuilder<Finisher> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.ToTable("Finishers");
        _ = builder.HasKey(finisher => finisher.Id);
        _ = builder.HasIndex(finisher => finisher.Name);
        _ = builder.HasIndex(finisher => finisher.Order);

        _ = builder.Property(finisher => finisher.UnlockItemIds)
            .HasJsonValueConversion();
    }
}
