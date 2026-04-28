using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Result;
using MediatR;

namespace Application.Features.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IHashService hashService)
        : IRequestHandler<ChangePasswordCommand, Response<bool>>
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IHashService _hashService = hashService;

        public async Task<Response<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            if (request.NewPassword != request.ConfirmPassword)
                return Response<bool>.Fail("New password and confirm password do not match.");

            if (request.NewPassword == request.OldPassword)
                return Response<bool>.Fail("New password must differ from the old password.");

            var user = await _currentUserService.GetCurrentUserAsync(cancellationToken)
                       ?? throw new UnauthorizedAccessException("Not authenticated.");

            if (!_hashService.Verify(request.OldPassword, user.PasswordHash))
                return Response<bool>.Fail("Old password is incorrect.");

            user.PasswordHash = _hashService.Hash(request.NewPassword);
            var updated = await _userRepository.UpdateAsync(user, cancellationToken);

            return updated != null
                ? Response<bool>.Ok(true, "Password changed successfully.")
                : Response<bool>.Fail("Failed to change password.");
        }
    }
}
