using Domain.Behaviours;
using Domain.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User : AudiTableEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? UserName { get; set; } = null;
        [PhoneNumberValidation]
        public string? Phone { get; set; }
        [EmailAddress]
        public string Email { get; set; } = "youremail@gmail.com";
        public string PasswordHash { get; set; } = null!;

        public Guid? FamilyId { get; set; }
        public Family? Family { get; set; }

        public Guid? ImageId { get; set; }
        public UploadedFile? Image { get; set; }

        public Guid RoleId { get; set; }
        public UserRole Role { get; set; } = null!;
    }
}
