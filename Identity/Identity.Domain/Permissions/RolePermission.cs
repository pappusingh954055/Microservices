using Identity.Domain.Entities;
using Identity.Domain.Roles;
using Identity.Domain.Menus;
using System.ComponentModel.DataAnnotations;
using Identity.Domain.Common;

namespace Identity.Domain.Permissions;

public class RolePermission : AuditableEntity
{
    [Key]
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int MenuId { get; set; }
    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }

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

    public RolePermission() { }

    public void UpdatePermissions(bool canView, bool canAdd, bool canEdit, bool canDelete)
    {
        CanView = canView;
        CanAdd = canAdd;
        CanEdit = canEdit;
        CanDelete = canDelete;
        SetModified();
    }
}
