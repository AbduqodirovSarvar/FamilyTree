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

namespace Application.Common.Models
{
    public record UserViewModel : BaseViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public Guid FamilyId { get; set; }
        public FamilyViewModel? Family { get; set; }

        public Guid RoleId { get; set; }
        public UserRoleViewModel? Role { get; set; }
    }
}
