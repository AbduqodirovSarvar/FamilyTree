using Application.Common.Models;
using Application.Common.Models.Dtos.UserRole;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserRole.Commands.Create
{
    public record CreateUserRoleCommand : CreateUserRoleDto, IRequest<Response<UserRoleViewModel>>
    {
    }
}
