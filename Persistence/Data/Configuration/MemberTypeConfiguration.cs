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

            // Father ↔ Children — bitta self-FK (FatherId), ikki tomondan navigatsiya.
            // Ilgari "Father" va "Children" alohida konfiguratsiya qilingan edi va
            // EF ikkinchi munosabat uchun shadow "FatherId1" ustun yaratayotgan edi.
            builder.HasOne(m => m.Father)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.FatherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mother relation
            builder.HasOne(m => m.Mother)
                .WithMany()
                .HasForeignKey(m => m.MotherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Spouse relation (self-reference, many-to-one).
            // Bir necha a'zo bir kishini SpouseId sifatida ko'rsatishi mumkin
            // (bir er bir nechta xotinga ega bo'lgan holatlar). 1-1 dan
            // ko'p-1 ga o'tildi va SpouseId ustidagi unique index olib tashlandi.
            builder.HasOne(m => m.Spouse)
                .WithMany()
                .HasForeignKey(m => m.SpouseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
