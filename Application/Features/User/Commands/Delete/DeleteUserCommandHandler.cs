using Application.Common.Interfaces;
using Application.Common.Interfaces.EntityServices;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Result;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.User.Commands.Delete
{
    public class DeleteUserCommandHandler(
        IMapper mapper,
        IUserService userService
        ) : IRequestHandler<DeleteUserCommand, Response<bool>>
    {
        private readonly IMapper _mapper = mapper;
        private readonly IUserService _userService = userService;
        public async Task<Response<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _userService.DeleteAsync(request.UserId, cancellationToken);
            if (result)
            {
                return Response<bool>.Ok(true, "User deleted successfully.");
            }

            return Response<bool>.Fail("Failed to delete user.");
        }
    }
}
