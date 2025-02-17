using GuildWars2.Hero.Equipment.JadeBots;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SL.ChatLinks.Storage.Models.Hero.Equipment.JadeBots;

public sealed class JadeBotSkinEntityTypeConfiguration : IEntityTypeConfiguration<JadeBotSkin>
{
    public void Configure(EntityTypeBuilder<JadeBotSkin> builder)
    {
        _ = builder.ToTable("JadeBots");
        _ = builder.HasKey(jadeBot => jadeBot.Id);
        _ = builder.HasIndex(jadeBot => jadeBot.Name);
        _ = builder.HasIndex(jadeBot => jadeBot.UnlockItemId);
    }
}
