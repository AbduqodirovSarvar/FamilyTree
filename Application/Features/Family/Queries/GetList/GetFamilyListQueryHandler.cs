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

namespace Application.Features.Family.Queries.GetList
{
    public class GetFamilyListQueryHandler(
        IFamilyService familyService
        ) : IRequestHandler<GetFamilyListQuery, Response<List<FamilyViewModel>>>
    {
        private readonly IFamilyService _familyService = familyService;
        public async Task<Response<List<FamilyViewModel>>> Handle(GetFamilyListQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<Domain.Entities.Family, bool>>? predicate = null;
            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var search = request.SearchText.ToLower();

                predicate = user =>
                    (user.Name != null && user.Name.ToLower().Contains(search)) ||
                    (user.FamilyName != null && user.FamilyName.ToLower().Contains(search)) ||
                    (user.Description != null && user.Description.ToLower().Contains(search));
            }

            return await _familyService.GetAllAsync(predicate, request.PageIndex, request.PageSize, cancellationToken);
        }
    }
}
