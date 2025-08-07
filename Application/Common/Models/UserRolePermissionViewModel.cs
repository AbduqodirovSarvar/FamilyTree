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
    public record UserRolePermissionViewModel : BaseViewModel
    {
        public Guid UserRoleId { get; set; }
        public UserRoleViewModel? UserRole { get; set; }
        public Permission Permission { get; set; } = Permission.GET_FAMILY;
    }
}
