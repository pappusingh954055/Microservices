using Identity.Application.Interfaces;
using Identity.Domain.Permissions;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Repositories;

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly IdentityDbContext _context;

    public RolePermissionRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RolePermission>> GetPermissionsByRoleIdAsync(int roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();
    }

    public async Task UpdateRolePermissionsAsync(int roleId, IEnumerable<RolePermission> permissions)
    {
        var existing = await GetPermissionsByRoleIdAsync(roleId);
        _context.RolePermissions.RemoveRange(existing);

        foreach (var perm in permissions)
        {
            perm.RoleId = roleId;
            await _context.RolePermissions.AddAsync(perm);
        }

        await _context.SaveChangesAsync();
    }
}
