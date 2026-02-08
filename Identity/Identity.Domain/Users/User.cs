using Identity.Domain.Entities;
using Identity.Domain.Users;

public class User
{
    private readonly List<UserRole> _userRoles = new();
    private readonly List<RefreshToken> _refreshTokens = new();

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { }

    public User(string userName, string email)
    {
        UserName = userName;
        Email = email;
    }

    public void SetPasswordHash(string hash)
    {
        PasswordHash = hash;
    }

    public void AssignRole(int roleId)
    {
        if (_userRoles.Any(r => r.RoleId == roleId))
            return;

        _userRoles.Add(new UserRole(Id, roleId));
    }

    // ✅ FIXED
    public void AddRefreshToken(string token, DateTime expiresAt)
    {
        _refreshTokens.Add(new RefreshToken(Id, token, expiresAt));
    }

    public void RevokeRefreshToken(string token)
    {
        var rt = _refreshTokens.Single(x => x.Token == token);
        rt.Revoke();
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}
