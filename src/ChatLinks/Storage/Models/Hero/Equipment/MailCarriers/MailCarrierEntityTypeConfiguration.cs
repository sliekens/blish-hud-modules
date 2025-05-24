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
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.ToTable("MailCarriers");
        _ = builder.HasKey(mailCarrier => mailCarrier.Id);
        _ = builder.HasIndex(mailCarrier => mailCarrier.Name);
        _ = builder.HasIndex(mailCarrier => mailCarrier.Order);

        builder.Property(mailCarrier => mailCarrier.UnlockItemIds)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new CollectionComparer<int>());

        _ = builder.Property(mailCarrier => mailCarrier.Flags)
            .HasJsonValueConversion();

#pragma warning disable CS0618 // Type or member is obsolete
        _ = builder.Ignore(mailCarrier => mailCarrier.IconHref);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
