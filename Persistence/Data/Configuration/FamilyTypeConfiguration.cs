using Domain.Entities;
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
        }
    }
}
