namespace Inventory.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }

    public string CategoryCode { get; private set; } = null!;
    public string CategoryName { get; private set; } = null!;
    public decimal DefaultGst { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    public DateTime? CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedOn { get; set; } = DateTime.UtcNow;
    public int? UpdatedBy { get; set; }
    public int? CreatedBy { get; set; }
    private Category() { } // EF Core

    public Category(
        string name,
        string code,        
        decimal defaultGst,
        string? description, 
        bool isActive)
    {
        Id = Guid.NewGuid();
        CategoryName = name;
        CategoryCode = code;        
        DefaultGst = defaultGst;
        Description = description;
        IsActive = isActive;
    }

    public void Update(
        string name,
        string code,        
        decimal defaultGst,
        string? description,
        bool isActive)
    {
        CategoryName = name;
        CategoryCode = code;        
        DefaultGst = defaultGst;
        Description = description;
        IsActive = isActive;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
