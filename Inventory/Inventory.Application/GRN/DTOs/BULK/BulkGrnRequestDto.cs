public class BulkGrnRequestDto
{
    public List<int> PurchaseOrderIds { get; set; } = new List<int>();
    public string CreatedBy { get; set; } = string.Empty;
}