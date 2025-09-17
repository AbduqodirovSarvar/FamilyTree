using Application.Common.Models.Dtos.Common;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Models.Dtos.UserRolePermission
{
    public record CreateUserRolePermissionDto : BaseCreateDto
    {
        public Guid UserRoleId { get; init; }
        public Permission Permission { get; init; } = Permission.GET_FAMILY;
    }
}
