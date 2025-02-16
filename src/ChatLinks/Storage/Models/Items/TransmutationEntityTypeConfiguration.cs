using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Items;

public sealed class TransmutationEntityTypeConfiguration : IEntityTypeConfiguration<Transmutation>
{
    public void Configure(EntityTypeBuilder<Transmutation> builder)
    {
        builder.Property(transmutation => transmutation.SkinIds)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new CollectionComparer<int>());
    }
}