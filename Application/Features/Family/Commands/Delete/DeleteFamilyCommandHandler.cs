using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Family.Commands.Delete
{
    public class DeleteFamilyCommandHandler(
        IFamilyService familyService
        ) : IRequestHandler<DeleteFamilyCommand, Response<bool>>
    {
        private readonly IFamilyService _familyService = familyService;
        public async Task<Response<bool>> Handle(DeleteFamilyCommand request, CancellationToken cancellationToken)
        {
            var result = await _familyService.DeleteAsync(request.Id, cancellationToken);
            if (result)
            {
                return Response<bool>.Ok(result);
            }

            return Response<bool>.Fail("Failed to delete family.");
        }
    }
}
