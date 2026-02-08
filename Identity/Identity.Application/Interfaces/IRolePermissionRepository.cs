using Identity.Domain.Permissions;

namespace Identity.Application.Interfaces;

public interface IRolePermissionRepository
{
    Task<IEnumerable<RolePermission>> GetPermissionsByRoleIdAsync(int roleId);
    Task UpdateRolePermissionsAsync(int roleId, IEnumerable<RolePermission> permissions);
}
