using System.ComponentModel.DataAnnotations;

namespace Identity.Domain.Menus;

public class Menu
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int? ParentId { get; set; }
    public int Order { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public Menu? Parent { get; private set; }
    public ICollection<Menu> Children { get; private set; } = new List<Menu>();

    public Menu() { }

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