using Application.Common.Models.Dtos.Auth;
using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Commands.Reset
{
    public record ResetSignInCommand : ResetSignInDto, IRequest<Response<bool>>
    {
    }
}
