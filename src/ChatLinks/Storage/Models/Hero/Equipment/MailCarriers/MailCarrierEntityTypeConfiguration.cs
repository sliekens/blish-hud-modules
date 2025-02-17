using GuildWars2.Hero.Equipment.MailCarriers;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Hero.Equipment.MailCarriers;

public sealed class MailCarrierEntityTypeConfiguration : IEntityTypeConfiguration<MailCarrier>
{
    public void Configure(EntityTypeBuilder<MailCarrier> builder)
    {
        _ = builder.ToTable("MailCarriers");
        _ = builder.HasKey(mailCarrier => mailCarrier.Id);
        _ = builder.HasIndex(mailCarrier => mailCarrier.Name);
        _ = builder.HasIndex(mailCarrier => mailCarrier.Order);

        builder.Property(mailCarrier => mailCarrier.UnlockItemIds)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new CollectionComparer<int>());

        _ = builder.Property(mailCarrier => mailCarrier.Flags)
            .HasJsonValueConversion();
    }
}
