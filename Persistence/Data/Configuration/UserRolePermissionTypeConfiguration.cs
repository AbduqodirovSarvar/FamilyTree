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
    public class UserRolePermissionTypeConfiguration : AudiTableEntityTypeConfiguration<UserRolePermission>
    {
        public override void Configure(EntityTypeBuilder<UserRolePermission> builder)
        {
            base.Configure(builder);
            builder.HasIndex(x => new { x.UserRoleId, x.Permission }).IsUnique();

            builder.HasOne(x => x.UserRole)
               .WithMany(r => r.Permissions)
               .HasForeignKey(x => x.UserRoleId)
               .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
