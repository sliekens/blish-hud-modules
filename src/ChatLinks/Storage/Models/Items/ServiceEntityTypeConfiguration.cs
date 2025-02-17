using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public sealed class ServiceEntityTypeConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        _ = builder.Property(service => service.Effect).HasColumnName("Effect").HasJsonValueConversion();
        _ = builder.Property(service => service.GuildUpgradeId).HasColumnName("GuildUpgradeId");
    }
}
