using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Commands.Confirmation
{
    public record SendConfirmationCodeCommand : IRequest<Response<bool>>
    {
        public required string Email { get; init; } = null!;
    }
}
