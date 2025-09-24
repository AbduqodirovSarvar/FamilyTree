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
    public record UserRolePermissionViewModel : BaseViewModel
    {
        public Guid UserRoleId { get; init; }
        public UserRoleViewModel? UserRole { get; init; }
        public EnumViewModel? Permission { get; init; }
    }
}
