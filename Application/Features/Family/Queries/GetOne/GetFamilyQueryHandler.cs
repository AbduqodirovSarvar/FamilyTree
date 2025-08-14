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

namespace Application.Features.Family.Queries.GetOne
{
    public class GetFamilyQueryHandler(
        IFamilyService familyService
        ) : IRequestHandler<GetFamilyQuery, Response<FamilyViewModel>>
    {
        private readonly IFamilyService _familyService = familyService;
        public async Task<Response<FamilyViewModel>> Handle(GetFamilyQuery request, CancellationToken cancellationToken)
        {
            FamilyViewModel? result = null;
            if (request.Id.HasValue && request.Id != Guid.Empty)
            {
                result = await _familyService.GetByIdAsync(request.Id.Value, cancellationToken)
                             ?? throw new KeyNotFoundException("Family not found.");
                return Response<FamilyViewModel>.Ok(result);
            }

            result = await _familyService.GetAsync(x => x.FamilyName == request.FamilyName, cancellationToken)
                             ?? throw new KeyNotFoundException("Family not found.");
            return Response<FamilyViewModel>.Ok(result);
        }
    }
}
