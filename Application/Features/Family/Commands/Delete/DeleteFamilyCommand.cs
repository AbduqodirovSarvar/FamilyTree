using Application.Common.Models.Dtos.Common;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Family.Commands.Delete
{
    public record DeleteFamilyCommand : BaseDeleteDto, IRequest<Response<bool>>
    {
    }
}
