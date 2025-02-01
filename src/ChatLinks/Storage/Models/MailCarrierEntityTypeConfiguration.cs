using GuildWars2.Hero.Equipment.JadeBots;
using GuildWars2.Hero.Equipment.MailCarriers;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models;

public sealed class MailCarrierEntityTypeConfiguration : IEntityTypeConfiguration<MailCarrier>
{
    public void Configure(EntityTypeBuilder<MailCarrier> builder)
    {
        builder.ToTable("MailCarriers");
        builder.HasKey(mailCarrier => mailCarrier.Id);
        builder.HasIndex(mailCarrier => mailCarrier.Name);
        builder.HasIndex(mailCarrier => mailCarrier.Order);

        builder.Property(mailCarrier => mailCarrier.UnlockItemIds)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new CollectionComparer<int>());

        builder.Property(mailCarrier => mailCarrier.Flags)
            .HasJsonValueConversion();
    }
}