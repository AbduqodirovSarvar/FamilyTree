using Application.Common.Models.Common;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models
{
    public record MemberViewModel : BaseViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Description { get; set; }
        public DateOnly BirthDay { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateOnly? DeathDay { get; set; }

        public Gender Gender { get; set; } = Gender.MALE;

        public Guid FamilyId { get; set; }
        public FamilyViewModel? Family { get; set; }

        public Guid? ImageId { get; set; }
        public UploadedFileViewModel? Image { get; set; }

        public Guid? FatherId { get; set; } = null;
        public MemberViewModel? Father { get; set; }

        public Guid? MotherId { get; set; } = null;
        public MemberViewModel? Mother { get; set; }

        public Guid? SpouseId { get; set; } = null;
        public MemberViewModel? Spouse { get; set; }

        public ICollection<MemberViewModel> Children { get; set; } = [];
    }
}
