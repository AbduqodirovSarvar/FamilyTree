using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using AutoMapper;
using MediatR;

namespace Application.Features.Auth.Commands.LeaveFamily
{
    /// <summary>
    /// Detaches the current user from their family by clearing
    /// <c>User.FamilyId</c>. Users who own a family cannot leave it — they must
    /// delete the family record instead, since leaving would orphan a tree
    /// the rest of the household still depends on.
    /// </summary>
    public class LeaveFamilyCommandHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IFamilyRepository familyRepository,
        IMapper mapper)
        : IRequestHandler<LeaveFamilyCommand, Response<UserViewModel>>
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IFamilyRepository _familyRepository = familyRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<UserViewModel>> Handle(LeaveFamilyCommand request, CancellationToken cancellationToken)
        {
            var user = await _currentUserService.GetCurrentUserAsync(cancellationToken)
                       ?? throw new UnauthorizedAccessException("Not authenticated.");

            if (!user.FamilyId.HasValue)
                throw new InvalidOperationException("Siz hech qaysi oilaga biriktirilmagansiz.");

            var family = await _familyRepository.GetByIdAsync(user.FamilyId.Value, cancellationToken)
                         ?? throw new KeyNotFoundException("Oila topilmadi.");

            // Owners can't leave their own family — leaving would strand the
            // tree they created. The intended escape hatch is "delete the
            // family", which is owner-only and tears down the whole record.
            if (family.OwnerId == user.Id)
                throw new InvalidOperationException("Oila egasi oiladan chiqa olmaydi. Avval oilani o'chiring.");

            user.FamilyId = null;

            var updated = await _userRepository.UpdateAsync(user, cancellationToken)
                          ?? throw new InvalidOperationException("Oiladan chiqishda xatolik yuz berdi.");

            return Response<UserViewModel>.Ok(_mapper.Map<UserViewModel>(updated), "Siz oiladan muvaffaqiyatli chiqdingiz.");
        }
    }
}
