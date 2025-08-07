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
    public record UserRoleViewModel : BaseViewModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? DesignedName { get; set; }

        public Guid? FamilyId { get; set; }
        public FamilyViewModel? Family { get; set; }

        public ICollection<Permission> Permissions { get; set; } = [];
    }
}
