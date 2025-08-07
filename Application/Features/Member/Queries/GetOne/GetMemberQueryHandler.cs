using Application.Common.Interfaces.EntityServices;
using Application.Common.Models;
using Application.Common.Models.Result;
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
            MemberViewModel? result;
            if (!request.Id.HasValue || request.Id == Guid.Empty)
            {
                return Response<MemberViewModel>.Fail("Member ID is required to retrieve member details.");
            }
            result = await _memberService.GetByIdAsync(request.Id.Value, cancellationToken)
                             ?? throw new KeyNotFoundException("Member not found.");

            return Response<MemberViewModel>.Ok(result, "Member retrieved successfully.");
        }
    }
}
