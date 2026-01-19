public class Supplier
{
    public int Id { get; private set; } // Private set for DDD encapsulation
    public string Name { get; private set; }
    public string Phone { get; private set; }
    public string? GstIn { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; }=true;
    public string? CreatetedBy { get; set; }
    public DateTime? CreatedDate { get; set; }=DateTime.Now;    
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }=DateTime.Now;


    private Supplier() { }

    // Constructor for DDD
    public Supplier(string name, string phone, string? gstin, string? address, string? createtedby)
    {
        Name = name;
        Phone = phone;
        GstIn = gstin;
        Address = address;
        CreatetedBy = createtedby;

    }

    // Method to update supplier details (DDD Logic)
    public void UpdateDetails(string name, string phone, string? gstIn, string? address)
    {
        Name = name;
        Phone = phone;
        GstIn = gstIn;
        Address = address;
        UpdatedDate = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;
}