using Application.Common.Models.Result;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.User.Commands.Delete
{
    public record DeleteUserCommand : IRequest<Response<bool>>
    {
        public Guid Id { get; set; }
    }
}
