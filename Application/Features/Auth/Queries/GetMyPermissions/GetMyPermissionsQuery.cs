using Application.Common.Models.Result;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.Auth.Queries.GetMyPermissions
{
    public record GetMyPermissionsQuery : IRequest<Response<List<string>>>;
}
