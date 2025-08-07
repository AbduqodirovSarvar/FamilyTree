using Application.Common.Interfaces.EntityServices;
using Application.Common.Models;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Family.Commands.Update
{
    public class UpdateFamilyCommandHandler(
        IFamilyService familyService
        ) : IRequestHandler<UpdateFamilyCommand, Response<FamilyViewModel>>
    {
        private readonly IFamilyService _familyService = familyService;
        public async Task<Response<FamilyViewModel>> Handle(UpdateFamilyCommand request, CancellationToken cancellationToken)
        {
            var result = await _familyService.UpdateAsync(request, cancellationToken)
                                ?? throw new InvalidOperationException("Family update failed.");

            return Response<FamilyViewModel>.Ok(result);
        }
    }
}
