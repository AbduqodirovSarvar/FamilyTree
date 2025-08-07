using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Member.Commands.Delete
{
    public class DeleteMemberCommandHandler(
        IMemberService memberService
        ) : IRequestHandler<DeleteMemberCommand, Response<bool>>
    {
        private readonly IMemberService _memberService = memberService;
        public async Task<Response<bool>> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
        {
            var result = await _memberService.DeleteAsync(request.Id, cancellationToken);
            if (result)
            {
                return Response<bool>.Ok(true, "Member deleted successfully.");
            }

            return Response<bool>.Fail("Failed to delete member.");
        }
    }
}
