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
    public class UploadedFileTypeConfiguration : AudiTableEntityTypeConfiguration<UploadedFile>
    {
        public override void Configure(EntityTypeBuilder<UploadedFile> builder)
        {
            base.Configure(builder);
            builder.HasIndex(f => f.Name).IsUnique();
            builder.HasIndex(f => f.Path).IsUnique();
            builder.HasIndex(f => f.Url).IsUnique();
        }
    }
}
