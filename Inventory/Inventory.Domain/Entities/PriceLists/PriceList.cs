namespace Inventory.Domain.PriceLists;

public class PriceList
{
    public Guid Id { get;  set; }
    public string Name { get; set; }
    public string Code { get;  set; }
    public string PriceType { get;  set; }
    public string ApplicableGroup { get;  set; } // UI: Apply to Group
    public string Currency { get;  set; }        // UI: Currency
    public string? Remarks { get;  set; }        // UI: Description/Remarks
    public DateTime ValidFrom { get;  set; }
    public DateTime? ValidTo { get;  set; }
    public bool IsActive { get;  set; }
    public DateTime? CreatedOn { get;  set; } = DateTime.Now;
    public string?  CreatedBy { get; set; }
    public DateTime? UpdatedOn { get;  set; } = DateTime.Now;
    public string? UpdatedBy { get;  set; }

    // Relationship
    public List<PriceListItem> PriceListItems { get;  set; } = new();

    private PriceList() { } // EF Core ke liye

    public PriceList(string name, string code, string priceType, string applicableGroup,
                     string currency, DateTime validFrom, DateTime? validTo,
                     string? remarks, bool isActive, string createdBy)
    {
        Id = Guid.NewGuid();
        Name = name;
        Code = code;
        PriceType = priceType;
        ApplicableGroup = applicableGroup;
        Currency = currency;
        ValidFrom = validFrom;
        ValidTo = validTo;
        Remarks = remarks;
        IsActive = isActive;
        CreatedBy = createdBy;
    }
}