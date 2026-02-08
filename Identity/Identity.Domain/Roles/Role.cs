namespace Identity.Domain.Roles;

public class Role
{
    public int Id { get; set; }

    public string RoleName { get; set; } = default!;

    private Role() { } // EF Core

    public Role(int id, string roleName)
    {
        Id = id;
        RoleName = roleName;
    }
}
