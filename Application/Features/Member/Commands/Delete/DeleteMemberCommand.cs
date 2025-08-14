using Application.Common.Models.Dtos.Common;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Member.Commands.Delete
{
    public record DeleteMemberCommand : BaseDeleteDto, IRequest<Response<bool>>
    {
    }
}
