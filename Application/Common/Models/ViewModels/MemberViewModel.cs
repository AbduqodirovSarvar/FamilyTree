using Application.Common.Models.Common;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.ViewModels
{
    public record MemberViewModel : BaseViewModel
    {
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? Description { get; init; }
        public DateOnly BirthDay { get; init; }
        public DateOnly? DeathDay { get; init; }
        public Gender Gender { get; init; }
        public Guid FamilyId { get; init; }
        public FamilyViewModel? Family { get; init; }
        public Guid? ImageId { get; init; }
        public UploadedFileViewModel? Image { get; init; }
        public Guid? FatherId { get; init; } = null;
        public MemberViewModel? Father { get; init; }
        public Guid? MotherId { get; init; } = null;
        public MemberViewModel? Mother { get; init; }
        public Guid? SpouseId { get; init; } = null;
        public MemberViewModel? Spouse { get; init; }
        public ICollection<MemberViewModel> Children { get; init; } = [];
    }
}
