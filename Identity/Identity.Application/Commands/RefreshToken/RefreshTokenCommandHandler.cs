using Identity.Application.Common;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using MediatR;

namespace Identity.Application.Commands.RefreshToken;

public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _uow;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        IUnitOfWork uow,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _uow = uow;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Result<AuthResponse>> Handle(
    RefreshTokenCommand request,
    CancellationToken ct)
    {
        var token = await _refreshTokenRepository.GetAsync(request.RefreshToken);
        if (token == null || !token.IsActive)
            return Result<AuthResponse>.Failure("Invalid refresh token");

        await _refreshTokenRepository.RevokeAllAsync(token.UserId);

        var user = await _userRepository.GetByIdAsync(token.UserId);
        var roles = user!.UserRoles.Select(r => r.Role.RoleName).ToList();

        var auth = _jwtService.Generate(user, roles);

        await _refreshTokenRepository.AddAsync(
            new Domain.Entities.RefreshToken(
                user.Id,
                auth.RefreshToken,
                auth.ExpiresAt.AddDays(7)));

        await _uow.SaveChangesAsync(ct);

        return Result<AuthResponse>.Success(auth);
    }

}
