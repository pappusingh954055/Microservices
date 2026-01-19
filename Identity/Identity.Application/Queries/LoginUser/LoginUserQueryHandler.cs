using Identity.Application.Common;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Queries.LoginUser;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

public class LoginUserQueryHandler
    : IRequestHandler<LoginUserQuery, Result<AuthResponse>>
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _tokens;
    private readonly IPasswordHasher<User> _hasher;
    private readonly IJwtService _jwt;
    private readonly IUnitOfWork _uow;

    public LoginUserQueryHandler(
        IUserRepository users,
        IRefreshTokenRepository tokens,
        IPasswordHasher<User> hasher,
        IJwtService jwt,
        IUnitOfWork uow)
    {
        _users = users;
        _tokens = tokens;
        _hasher = hasher;
        _jwt = jwt;
        _uow = uow;
    }

    public async Task<Result<AuthResponse>> Handle(
    LoginUserQuery request,
    CancellationToken ct)
    {
        var user = await _users.GetWithRolesByEmailAsync(request.Dto.Email);
        if (user == null)
            return Result<AuthResponse>.Failure("Invalid credentials");

        var verify = _hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Dto.Password);

        if (verify == PasswordVerificationResult.Failed)
            return Result<AuthResponse>.Failure("Invalid credentials");

        // ✅ SAFE: bulk revoke
        await _tokens.RevokeAllAsync(user.Id);

        var roles = user.UserRoles
            .Select(r => r.Role.RoleName)
            .ToList();

        // 1. Generate Auth object (Yahan check karein ki Generate method ID set karta hai ya nahi)
        var auth = _jwt.Generate(user, roles);

        // 2. Explicitly mapping UserId (AGAR auth.UserId zero/empty aa raha hai toh ye line zaroori hai)
        auth.UserId = user.Id;
        // Agar user.Id pehle se string/guid hai toh seedha set karein: auth.UserId = user.Id;

        await _tokens.AddAsync(
            new RefreshToken(
                user.Id,
                auth.RefreshToken,
                auth.ExpiresAt.AddDays(7)));

        await _uow.SaveChangesAsync(ct);

        // 3. Return the fully mapped response
        return Result<AuthResponse>.Success(auth);
    }
}
