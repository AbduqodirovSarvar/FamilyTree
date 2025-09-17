using Domain.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Data.Configuration.Common
{
    public abstract class AudiTableEntityTypeConfiguration<TEntity> : BaseEntityTypeConfiguration<TEntity>
        where TEntity : AudiTableEntity
    {
        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            base.Configure(builder);
        }
    }
}
