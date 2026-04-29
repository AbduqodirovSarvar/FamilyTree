using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Extentions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MemberEntity = Domain.Entities.Member;

namespace Application.Features.Member.Queries.GetList
{
    public class GetMemberListQueryHandler(
        IMemberService memberService
        ) : IRequestHandler<GetMemberListQuery, Response<List<MemberViewModel>>>
    {
        private readonly IMemberService _memberService = memberService;

        public async Task<Response<List<MemberViewModel>>> Handle(GetMemberListQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<MemberEntity, bool>>? predicate =
                request.Filters.BuildPredicate<MemberEntity>()
                    .AndAlso(FilterExpressionBuilder.BuildSearchPredicate<MemberEntity>(
                        request.SearchText,
                        nameof(MemberEntity.FirstName),
                        nameof(MemberEntity.LastName),
                        nameof(MemberEntity.Description)));

            return await _memberService.GetAllAsync(predicate, request.PageIndex, request.PageSize, cancellationToken);
        }
    }
}
