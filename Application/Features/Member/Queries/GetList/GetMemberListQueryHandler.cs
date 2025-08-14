using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Extentions;
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
            if (request.Filters != null && request.Filters.Any())
            {
                predicate = request.Filters.BuildPredicate<Domain.Entities.Member>();
            }

            return await _memberService.GetAllAsync(predicate, request.PageIndex, request.PageSize, cancellationToken);
        }
    }
}
