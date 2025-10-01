using Application.Common.Models.Dtos.Auth;
using Application.Common.Models.Dtos.User;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Commands.SignUp
{
    public record SignUpCommand : CreateUserDto, IRequest<Response<bool>>
    {
    }
}
