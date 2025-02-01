using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SL.ChatLinks.Storage.Models;

public sealed class GizmoEntityTypeConfiguration : IEntityTypeConfiguration<Gizmo>
{
    public void Configure(EntityTypeBuilder<Gizmo> builder)
    {
        builder.Property(gizmo => gizmo.GuildUpgradeId).HasColumnName("GuildUpgradeId");
    }
}