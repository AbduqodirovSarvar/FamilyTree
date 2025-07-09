using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User : AudiTableEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Description { get; set; } = null;
        public Gender Gender { get; set; } = Gender.Male;
        public Guid FamilyId { get; set; }
        public Family? Family { get; set; }
        public DateOnly BirthDay { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateOnly? DeathDay { get; set; } = null;
        public Guid? ImageId { get; set; }
        public UploadedFile? Image { get; set; }
        public Guid FatherId { get; set; }
        public User? Father { get; set; }
        public Guid MotherId { get; set; }
        public User? Mother { get; set; }
        public Guid SpouseId { get; set; }
        public User? Spouse { get; set; }
    }
}
