using GuildWars2.Hero.Equipment.Dyes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SL.ChatLinks.Storage.Models;

public sealed class DyeColorEntityTypeConfiguration : IEntityTypeConfiguration<DyeColor>
{
    public void Configure(EntityTypeBuilder<DyeColor> builder)
    {
        builder.ToTable("dyes");
        builder.HasKey(x => x.Id);

    }
}