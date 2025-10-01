using Application.Common.Models.Common;
using Domain.Behaviours;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.ViewModels
{
    public record UserViewModel : BaseViewModel
    {
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? UserName { get; init; }
        public string? Phone { get; init; }
        public string? Email { get; init; }

        public Guid FamilyId { get; init; }
        public FamilyViewModel? Family { get; init; }

        public Guid? ImageId { get; set; }
        public UploadedFileViewModel? Image { get; set; }

        public Guid RoleId { get; init; }
        public UserRoleViewModel? Role { get; init; }
    }
}
