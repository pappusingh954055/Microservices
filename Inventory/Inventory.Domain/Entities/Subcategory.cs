namespace Inventory.Domain.Entities;

public sealed class Subcategory
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; }
    public int? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; } = DateTime.Now;
    public DateTime? ModifiedOn { get; set; } = DateTime.UtcNow;
    public int? ModifiedBy { get; set; }
    public string? SubcategoryCode { get; set; } = null!;
    public string? SubcategoryName { get; set; }=null!;
    public decimal DefaultGst { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    private Subcategory() { }

    public Subcategory(
        Guid categoryid,
        string? code,
        string name,
        decimal defaultGst,
        string? description,
        bool isactive)
    {
        Id = Guid.NewGuid();
        CategoryId = categoryid;
        SubcategoryCode = code;
        SubcategoryName = name;
        DefaultGst = defaultGst;
        Description = description;
        IsActive = isactive;
    }

    public void Update(
        string? code,
        string name,
        Guid categoryid,
        decimal defaultgst,
        string? description,
        bool isActive)
    {
        SubcategoryCode = code;
        SubcategoryName = name;
        CategoryId = categoryid;
        DefaultGst = defaultgst;
        Description = description;
        IsActive = isActive;
    }
}
