using Application.Common.Models.Dtos.Common;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserRolePermission.Commands.Delete
{
    public record DeleteUserRolePermissionCommand : BaseDeleteDto, IRequest<Response<bool>>
    {
    }
}
