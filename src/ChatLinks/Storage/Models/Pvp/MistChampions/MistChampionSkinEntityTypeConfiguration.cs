using GuildWars2.Pvp.MistChampions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SL.ChatLinks.Storage.Comparers;
using SL.ChatLinks.Storage.Converters;

namespace SL.ChatLinks.Storage.Models.Pvp.MistChampions;

public sealed class MistChampionSkinEntityTypeConfiguration : IEntityTypeConfiguration<MistChampionSkin>
{
    public void Configure(EntityTypeBuilder<MistChampionSkin> builder)
    {
        ThrowHelper.ThrowIfNull(builder);
        _ = builder.ToTable("MistChampions");
        _ = builder.HasKey(mistChampion => mistChampion.Id);
        _ = builder.HasIndex(mistChampion => mistChampion.Name);

        builder.Property(mistChampion => mistChampion.UnlockItemIds)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new CollectionComparer<int>());

#pragma warning disable CS0618 // Type or member is obsolete
        _ = builder.Ignore(mistChampion => mistChampion.IconHref);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
