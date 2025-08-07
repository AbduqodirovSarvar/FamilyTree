using Application.Common.Models;
using Application.Common.Models.Dtos.Family;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Family.Commands.Update
{
    public record UpdateFamilyCommand : UpdateFamilyDto, IRequest<Response<FamilyViewModel>>
    {
    }
}
