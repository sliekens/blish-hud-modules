using GuildWars2.Hero.Equipment.Finishers;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models;

public sealed class FinisherEntityTypeConfiguration : IEntityTypeConfiguration<Finisher>
{
    public void Configure(EntityTypeBuilder<Finisher> builder)
    {
        builder.ToTable("Finishers");
        builder.HasKey(finisher => finisher.Id);
        builder.HasIndex(finisher => finisher.Name);
        builder.HasIndex(finisher => finisher.Order);

        builder.Property(finisher => finisher.UnlockItemIds)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new CollectionComparer<int>());
    }
}