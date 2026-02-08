using Identity.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Identity.Domain.Menus;

public class Menu : AuditableEntity
{
    [Key]
    public int Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string? Icon { get; private set; }
    public int? ParentId { get; private set; }
    public int Order { get; private set; }

    public Menu? Parent { get; private set; }
    public ICollection<Menu> Children { get; private set; } = new List<Menu>();

    private Menu() { }

    public Menu(string title, string url, string? icon, int? parentId, int order)
    {
        Title = title;
        Url = url;
        Icon = icon;
        ParentId = parentId;
        Order = order;
    }
    public void Update(string title, string url, string? icon, int? parentId, int order)
    {
        Title = title;
        Url = url;
        Icon = icon;
        ParentId = parentId;
        Order = order;
    }
}