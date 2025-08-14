using Application.Common.Models.Dtos.UserRolePermission;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserRolePermission.Commands.Update
{
    public record UpdateUserRolePermissionCommand : UpdateUserRolePermissionDto, IRequest<Response<UserRolePermissionViewModel>>
    {
    }
}
