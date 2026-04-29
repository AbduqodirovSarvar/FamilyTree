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
using FamilyEntity = Domain.Entities.Family;

namespace Application.Features.Family.Queries.GetList
{
    public class GetFamilyListQueryHandler(
        IFamilyService familyService
        ) : IRequestHandler<GetFamilyListQuery, Response<List<FamilyViewModel>>>
    {
        private readonly IFamilyService _familyService = familyService;

        public async Task<Response<List<FamilyViewModel>>> Handle(GetFamilyListQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<FamilyEntity, bool>>? predicate =
                request.Filters.BuildPredicate<FamilyEntity>()
                    .AndAlso(FilterExpressionBuilder.BuildSearchPredicate<FamilyEntity>(
                        request.SearchText,
                        nameof(FamilyEntity.Name),
                        nameof(FamilyEntity.FamilyName),
                        nameof(FamilyEntity.Description)));

            return await _familyService.GetAllAsync(predicate, request.PageIndex, request.PageSize, cancellationToken);
        }
    }
}
