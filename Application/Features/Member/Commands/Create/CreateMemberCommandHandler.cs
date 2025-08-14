using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Member.Commands.Create
{
    public class CreateMemberCommandHandler(
        IMemberService memberService
        ) : IRequestHandler<CreateMemberCommand, Response<MemberViewModel>>
    {
        private readonly IMemberService _memberService = memberService;
        public async Task<Response<MemberViewModel>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
        {
            var result = await _memberService.CreateAsync(request, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to create member.");

            return Response<MemberViewModel>.Ok(result, "Member created successfully.");
        }
    }
}
