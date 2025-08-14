using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Services.EntityServices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Member.Queries.GetOne
{
    public class GetMemberQueryHandler(
        IMemberService memberService
        ) : IRequestHandler<GetMemberQuery, Response<MemberViewModel>>
    {
        private readonly IMemberService _memberService = memberService;
        public async Task<Response<MemberViewModel>> Handle(GetMemberQuery request, CancellationToken cancellationToken)
        {
            var result = await _memberService.GetByIdAsync(request.Id, cancellationToken)
                             ?? throw new KeyNotFoundException("Member not found.");

            return Response<MemberViewModel>.Ok(result, "Member retrieved successfully.");
        }
    }
}
