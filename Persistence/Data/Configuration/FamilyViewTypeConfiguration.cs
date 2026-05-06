using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Data.Configuration.Common;

namespace Persistence.Data.Configuration
{
    public class FamilyViewTypeConfiguration : BaseEntityTypeConfiguration<FamilyView>
    {
        public override void Configure(EntityTypeBuilder<FamilyView> builder)
        {
            base.Configure(builder);

            builder.Property(v => v.IpAddress)
                .HasMaxLength(64)
                .IsRequired();

            // Dedup index: one row per (family, ip, day). Anonymous refreshes
            // hit the upsert path that swallows the duplicate-key violation.
            builder.HasIndex(v => new { v.FamilyId, v.IpAddress, v.ViewDate })
                .IsUnique();

            // Range queries by family + date drive the stats endpoint —
            // index covers the common where/group-by pattern.
            builder.HasIndex(v => new { v.FamilyId, v.ViewDate });

            builder.HasOne(v => v.Family)
                .WithMany()
                .HasForeignKey(v => v.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
