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
        _ = builder.ToTable("MistChampions");
        _ = builder.HasKey(mistChampion => mistChampion.Id);
        _ = builder.HasIndex(mistChampion => mistChampion.Name);

        builder.Property(mistChampion => mistChampion.UnlockItemIds)
            .HasJsonValueConversion()
            .Metadata.SetValueComparer(new CollectionComparer<int>());
    }
}
