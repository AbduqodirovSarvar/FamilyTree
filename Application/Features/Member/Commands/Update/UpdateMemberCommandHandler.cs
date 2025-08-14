using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Member.Commands.Update
{
    public class UpdateMemberCommandHandler(
        IMemberService memberService
        ) : IRequestHandler<UpdateMemberCommand, Response<MemberViewModel>>
    {
        private readonly IMemberService _memberService = memberService;
        public async Task<Response<MemberViewModel>> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
        {
            var result = await _memberService.UpdateAsync(request, cancellationToken)
                                ?? throw new InvalidOperationException("Member update failed.");

            return Response<MemberViewModel>.Ok(result, "Member updated successfully.");
        }
    }
}
