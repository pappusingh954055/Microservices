namespace Identity.Domain.Roles;

public class Role
{
    public int Id { get; private set; }

    // ✅ MUST be settable for EF seeding
    public string RoleName { get; private set; } = default!;

    private Role() { } // EF Core

    public Role(int id, string roleName)
    {
        Id = id;
        RoleName = roleName;
    }
}
