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
    public record UserRoleViewModel : BaseViewModel
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
        public string? DesignedName { get; init; }

        public Guid? FamilyId { get; init; }
        public FamilyViewModel? Family { get; init; }

        public ICollection<EnumViewModel> Permissions { get; init; } = [];
    }
}
