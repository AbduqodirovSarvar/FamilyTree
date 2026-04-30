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
    public class UserTypeConfiguration : AudiTableEntityTypeConfiguration<User>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);
            builder.HasIndex(u => u.Phone).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.UserName).IsUnique();

            // User ↔ Family munosabati FamilyTypeConfiguration'da SetNull bilan
            // konfiguratsiya qilingan. Bu yerda takroriy konfiguratsiya bo'lsa,
            // OnDelete bahsi runtime model'ni snapshot'dan farq qildirib,
            // PendingModelChangesWarning'ga olib kelar edi.
        }
    }
}
