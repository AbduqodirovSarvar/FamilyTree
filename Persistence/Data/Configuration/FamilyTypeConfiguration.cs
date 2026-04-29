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

            // Owner ↔ Family — bitta foydalanuvchi bir nechta oilaga ega bo'la
            // oladi (bobosining ham o'zining ham daraxtini boshqarayotgan
            // holatlar). 1-1 dan ko'p-1 ga o'tildi va OwnerId ustidagi unique
            // index olib tashlandi.
            builder.HasOne(f => f.Owner)
                .WithMany()
                .HasForeignKey(f => f.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Family → Users — oila o'chsa, foydalanuvchilar tizimda qoladi,
            // shunchaki ularning FamilyId si tozalanadi. Cascade User'ni ham
            // o'chirar edi va bu Family.OwnerId bilan circular FK yarata edi
            // (oila egasi shu oilada bo'lganida delete'da xato chiqar edi).
            builder.HasMany(f => f.Users)
                .WithOne(u => u.Family)
                .HasForeignKey(u => u.FamilyId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
