using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using Application.Features.UploadedFile.Commands.Create;
using AutoMapper;
using MediatR;

namespace Application.Features.Auth.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IMediator mediator,
        IMapper mapper)
        : IRequestHandler<UpdateProfileCommand, Response<UserViewModel>>
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IMediator _mediator = mediator;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<UserViewModel>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _currentUserService.GetCurrentUserAsync(cancellationToken)
                       ?? throw new UnauthorizedAccessException("Not authenticated.");

            if (!string.IsNullOrWhiteSpace(request.FirstName)) user.FirstName = request.FirstName;
            if (!string.IsNullOrWhiteSpace(request.LastName)) user.LastName = request.LastName;
            if (!string.IsNullOrWhiteSpace(request.UserName))
            {
                // Registrdan qat'i nazar boshqa foydalanuvchi bilan to'qnashmasligini tekshiramiz.
                var normalizedUserName = request.UserName.Trim().ToLower();
                if (await _userRepository.AnyAsync(
                        u => u.Id != user.Id && u.UserName!.ToLower() == normalizedUserName,
                        cancellationToken))
                    throw new InvalidOperationException("Bu foydalanuvchi nomi allaqachon band. Iltimos, boshqa nom tanlang.");
                user.UserName = request.UserName;
            }
            if (!string.IsNullOrWhiteSpace(request.Phone)) user.Phone = request.Phone;
            if (!string.IsNullOrWhiteSpace(request.Email)) user.Email = request.Email;

            if (request.Image != null)
            {
                var imageResponse = await _mediator.Send(new CreateUploadedFileCommand
                {
                    File = request.Image,
                    Alt = user.FirstName,
                    Description = null
                }, cancellationToken)
                                ?? throw new InvalidOperationException("Couldn't save the image file.");

                if (imageResponse.Data != null)
                    user.ImageId = imageResponse.Data.Id;
            }

            var updated = await _userRepository.UpdateAsync(user, cancellationToken)
                          ?? throw new InvalidOperationException("Failed to update profile.");

            return Response<UserViewModel>.Ok(_mapper.Map<UserViewModel>(updated), "Profile updated successfully.");
        }
    }
}
