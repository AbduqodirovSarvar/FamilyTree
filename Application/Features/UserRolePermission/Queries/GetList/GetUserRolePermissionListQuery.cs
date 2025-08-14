using Application.Common.Models.Request;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.UserRolePermission.Queries.GetList
{
    public record GetUserRolePermissionListQuery : BaseGetListQuery, IRequest<Response<List<UserRolePermissionViewModel>>>
    {
    }
}
