using Application.Common.Models;
using Application.Common.Models.Dtos.Member;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Member.Commands.Create
{
    public record CreateMemberCommand : CreateMemberDto, IRequest<Response<MemberViewModel>>
    {
    }
}
