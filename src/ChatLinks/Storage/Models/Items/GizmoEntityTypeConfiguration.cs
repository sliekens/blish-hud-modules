using GuildWars2.Items;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SL.ChatLinks.Storage.Models.Items;

public sealed class GizmoEntityTypeConfiguration : IEntityTypeConfiguration<Gizmo>
{
    public void Configure(EntityTypeBuilder<Gizmo> builder)
    {
        _ = builder.Property(gizmo => gizmo.GuildUpgradeId).HasColumnName("GuildUpgradeId");
    }
}
