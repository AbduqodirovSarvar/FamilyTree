using Application.Common.Interfaces.EntityServices;
using Application.Common.Models.Result;
using MediatR;
using System.Linq.Expressions;

namespace Application.Features.User.Queries.CheckExist
{
    public class CheckUserExistQueryHandler(IUserService userService)
        : IRequestHandler<CheckUserExistQuery, Response<bool>>
    {
        private readonly IUserService _userService = userService;

        public async Task<Response<bool>> Handle(CheckUserExistQuery request, CancellationToken cancellationToken)
        {
            bool hasId = request.Id != null && request.Id != Guid.Empty;
            bool hasUserName = !string.IsNullOrWhiteSpace(request.UserName);
            bool hasEmail = !string.IsNullOrWhiteSpace(request.Email);
            bool hasPhone = !string.IsNullOrWhiteSpace(request.Phone);

            if (!hasId && !hasUserName && !hasEmail && !hasPhone)
                return Response<bool>.Fail("At least one of Id, UserName, Email, or Phone must be provided.");

            // UserName'ni registrga sezgir bo'lmagan holda solishtiramiz —
            // "Xolmurodov" va "xolmurodov" bir xil foydalanuvchi nomi.
            var normalizedUserName = hasUserName ? request.UserName!.Trim().ToLower() : null;

            Expression<Func<Domain.Entities.User, bool>> predicate = x =>
                (hasId && x.Id == request.Id!.Value)
                || (normalizedUserName != null && x.UserName!.ToLower() == normalizedUserName)
                || (hasEmail && x.Email == request.Email)
                || (hasPhone && x.Phone == request.Phone);

            var exists = await _userService.ExistsAsync(predicate, cancellationToken);
            return Response<bool>.Ok(exists);
        }
    }
}
