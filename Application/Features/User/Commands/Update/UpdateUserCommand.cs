using Application.Common.Models;
using Application.Common.Models.Dtos.User;
using Application.Common.Models.Result;
using Domain.Behaviours;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.User.Commands.Update
{
    public record UpdateUserCommand : UpdateUserDto, IRequest<Response<UserViewModel>>
    {
    }
}
