using Application.Common.Interfaces;
using Application.Common.Models.Result;
using Application.Common.Models.ViewModels;
using AutoMapper;
using MediatR;

namespace Application.Features.Auth.Queries.GetMe
{
    public class GetMeQueryHandler(
        ICurrentUserService currentUserService,
        IMapper mapper)
        : IRequestHandler<GetMeQuery, Response<UserViewModel>>
    {
        private readonly ICurrentUserService _currentUserService = currentUserService;
        private readonly IMapper _mapper = mapper;

        public async Task<Response<UserViewModel>> Handle(GetMeQuery request, CancellationToken cancellationToken)
        {
            var user = await _currentUserService.GetCurrentUserAsync(cancellationToken)
                       ?? throw new UnauthorizedAccessException("Not authenticated.");

            return Response<UserViewModel>.Ok(_mapper.Map<UserViewModel>(user));
        }
    }
}
