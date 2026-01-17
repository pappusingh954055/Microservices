using Identity.Domain.Roles;

namespace Identity.Application.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string roleName);
    Task<Role?> GetByIdAsync(int id);
}
