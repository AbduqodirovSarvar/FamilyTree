using Application.Common.Interfaces.EntityServices;
using Application.Common.Models;
using Application.Common.Models.Result;
using Application.Services.EntityServices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Member.Queries.GetList
{
    public class GetMemberListQueryHandler(
        IMemberService memberService
        ) : IRequestHandler<GetMemberListQuery, Response<List<MemberViewModel>>>
    {
        private readonly IMemberService _memberService = memberService;
        public async Task<Response<List<MemberViewModel>>> Handle(GetMemberListQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<Domain.Entities.Member, bool>>? predicate = null;
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var search = request.SearchText.ToLower();

                predicate = user =>
                    (user.FirstName != null && user.FirstName.ToLower().Contains(search)) ||
                    (user.LastName != null && user.LastName.ToLower().Contains(search)) ||
                    (user.Description != null && user.Description.ToLower().Contains(search));
            }

            return await _memberService.GetAllAsync(predicate, request.PageIndex, request.PageSize, cancellationToken);
        }
    }
}
