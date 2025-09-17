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
    public class MemberTypeConfiguration : AudiTableEntityTypeConfiguration<Member>
    {
        public override void Configure(EntityTypeBuilder<Member> builder)
        {
            base.Configure(builder);
        }
    }
}
