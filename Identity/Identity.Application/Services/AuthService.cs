
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Users;
using Microsoft.AspNetCore.Identity;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(
        IUserRepository users,
        IPasswordHasher<User> passwordHasher,
        IJwtService jwtService)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> LoginAsync(LoginDto dto)
    {
        var user = await _users.GetWithRolesByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        // ✅ VERIFY PASSWORD HERE
        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            dto.Password
        );

        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid credentials");

        var roles = user.UserRoles
            .Select(ur => ur.Role.RoleName)
            .ToList();

        return _jwtService.Generate(user, roles);
    }
}
