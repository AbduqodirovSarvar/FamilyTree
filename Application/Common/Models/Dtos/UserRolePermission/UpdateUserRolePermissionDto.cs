using Application.Common.Models.Dtos.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.Dtos.UserRolePermission
{
    public record UpdateUserRolePermissionDto : BaseUpdateDto
    {
        public Guid? UserRoleId { get; init; } = null;
        public Permission? Permission { get; init; } = null;
    }
}
