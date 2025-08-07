using Application.Common.Models;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Member.Queries.GetOne
{
    public class GetMemberQuery : IRequest<Response<MemberViewModel>>
    {
        public Guid? Id { get; init; } = null;
    }
}
