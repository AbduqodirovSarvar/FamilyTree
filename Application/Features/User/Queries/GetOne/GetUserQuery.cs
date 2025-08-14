using Application.Common.Models.Request;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.User.Queries.GetOne
{
    public record GetUserQuery : BaseGetOneQuery, IRequest<Response<UserViewModel>>
    {
        public new Guid? Id { get; init; } = null;
        public string? Email { get; init; } = null;
        public string? UserName { get; init; } = null;
        public string? Phone { get; init; } = null;
    }
}
