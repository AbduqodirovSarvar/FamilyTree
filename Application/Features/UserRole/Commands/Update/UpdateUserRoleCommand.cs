using Application.Common.Models.Dtos.UserRole;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserRole.Commands.Update
{
    public record UpdateUserRoleCommand : UpdateUserRoleDto, IRequest<Response<UserRoleViewModel>>
    {
    }
}
