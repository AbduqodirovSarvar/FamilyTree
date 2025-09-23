using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Data.Configuration.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.Configuration
{
    public class FamilyTypeConfiguration : AudiTableEntityTypeConfiguration<Family>
    {
        public override void Configure(EntityTypeBuilder<Family> builder)
        {
            base.Configure(builder);
            builder.HasIndex(f => f.FamilyName).IsUnique();
            builder.HasIndex(f => f.ImageId).IsUnique();

            builder.HasOne(f => f.Owner)
                .WithOne() // Ownerning alohida "OwnedFamilies" kolleksiyasi yo‘q
                .HasForeignKey<Family>(f => f.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(f => f.Users)
                .WithOne(u => u.Family)
                .HasForeignKey(u => u.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
