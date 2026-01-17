using Identity.Application.Common;
using Identity.Application.Interfaces;
using MediatR;

namespace Identity.Application.Commands.Logout;

public class LogoutCommandHandler
    : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _uow;

    public LogoutCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _uow = uow;
    }

    public async Task<Result<bool>> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);

        if (user == null)
            return Result<bool>.Failure("User not found");

        // 🔐 revoke only the provided refresh token
        user.RevokeRefreshToken(request.RefreshToken);

        await _uow.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
