using Application.Common.Interfaces.EntityServices;
using Application.Common.Models;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Family.Commands.Create
{
    public class CreateFamilyCommandHandler(
        IFamilyService familyService
        ) : IRequestHandler<CreateFamilyCommand, Response<FamilyViewModel>>
    {
        private readonly IFamilyService _familyService = familyService;
        public async Task<Response<FamilyViewModel>> Handle(CreateFamilyCommand request, CancellationToken cancellationToken)
        {
            var result = await _familyService.CreateAsync(request, cancellationToken)
                                ?? throw new InvalidOperationException("Failed to create family.");

            return Response<FamilyViewModel>.Ok(result);
        }
    }
}
