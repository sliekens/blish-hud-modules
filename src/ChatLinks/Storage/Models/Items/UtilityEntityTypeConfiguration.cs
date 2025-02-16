using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public sealed class UtilityEntityTypeConfiguration : IEntityTypeConfiguration<Utility>
{
    public void Configure(EntityTypeBuilder<Utility> builder)
    {
        builder.Property(utility => utility.Effect).HasColumnName("Effect").HasJsonValueConversion();
    }
}