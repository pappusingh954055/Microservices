using Identity.Domain.Entities;
using Identity.Domain.Roles;
using Identity.Domain.Menus;
using System.ComponentModel.DataAnnotations;

namespace Identity.Domain.Permissions;

public class RolePermission
{
    [Key]
    public int Id { get; set; }
    public int RoleId { get; private set; }
    public int MenuId { get; private set; }
    public bool CanView { get; private set; }
    public bool CanAdd { get; private set; }
    public bool CanEdit { get; private set; }
    public bool CanDelete { get; private set; }

    public Role Role { get; private set; } = default!;
    public Menu Menu { get; private set; } = default!;

    public RolePermission(int roleId, int menuId, bool canView, bool canAdd, bool canEdit, bool canDelete)
    {
        RoleId = roleId;
        MenuId = menuId;
        CanView = canView;
        CanAdd = canAdd;
        CanEdit = canEdit;
        CanDelete = canDelete;
    }

    private RolePermission() { }

    public void UpdatePermissions(bool canView, bool canAdd, bool canEdit, bool canDelete)
    {
        CanView = canView;
        CanAdd = canAdd;
        CanEdit = canEdit;
        CanDelete = canDelete;
    }
}