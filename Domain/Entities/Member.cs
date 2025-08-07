using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Member : AudiTableEntity
    {
        public string? FirstName { get; set; } = null!;
        public string? LastName { get; set; } = null;
        public string? Description { get; set; } = null;
        public DateOnly BirthDay { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateOnly? DeathDay { get; set; } = null;

        public Gender Gender { get; set; } = Gender.MALE;

        public Guid FamilyId { get; set; }
        public Family? Family { get; set; }

        public Guid? ImageId { get; set; }
        public UploadedFile? Image { get; set; }

        public Guid? FatherId { get; set; } = null;
        public Member? Father { get; set; }

        public Guid? MotherId { get; set; } = null;
        public Member? Mother { get; set; }

        public Guid? SpouseId { get; set; } = null;
        public Member? Spouse { get; set; }

        public ICollection<Member> Children { get; set; } = [];
    }
}
