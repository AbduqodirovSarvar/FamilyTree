using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models
{
    public class UserViewModel
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Description { get; set; } = null;
        public Gender Gender { get; set; } = Gender.Male;
        public Guid FamilyId { get; set; }
        public FamilyViewModel? Family { get; set; }
        public DateOnly BirthDay { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateOnly? DeathDay { get; set; } = null;
        public Guid? ImageId { get; set; }
        public UploadedFile? Image { get; set; }
        public Guid FatherId { get; set; }
        public UserViewModel? Father { get; set; }
        public Guid MotherId { get; set; }
        public UserViewModel? Mother { get; set; }
        public Guid SpouseId { get; set; }
        public UserViewModel? Spouse { get; set; }

        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
