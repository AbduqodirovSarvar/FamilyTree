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
    public class MemberTypeConfiguration : AudiTableEntityTypeConfiguration<Member>
    {
        public override void Configure(EntityTypeBuilder<Member> builder)
        {
            base.Configure(builder);

            // Family → Members (1-to-many)
            builder.HasOne(m => m.Family)
                .WithMany(f => f.Members)
                .HasForeignKey(m => m.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);

            // Father relation
            builder.HasOne(m => m.Father)
                .WithMany() // farzandlar "Children"da saqlanadi
                .HasForeignKey(m => m.FatherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mother relation
            builder.HasOne(m => m.Mother)
                .WithMany()
                .HasForeignKey(m => m.MotherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Spouse relation (self-reference 1-1)
            builder.HasOne(m => m.Spouse)
                .WithOne() // reciprocal navigation qo‘ymading, shuning uchun WithOne()
                .HasForeignKey<Member>(m => m.SpouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Children relation (1-to-many)
            builder.HasMany(m => m.Children)
                .WithOne() // parent-childni aniq ko‘rsatmagan, lekin ishlaydi
                .HasForeignKey(c => c.FatherId) // yoki c => c.MotherId qilib ajratib ko‘rsatishing mumkin
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
