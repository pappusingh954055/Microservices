namespace Inventory.Application.DTOs.SaleOrder;

public class SaleOrderItemGridDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty; // Table "Product" column
    public decimal SoldQty { get; set; } // Table "Sold Qty" column
    public decimal Rate { get; set; } // Table "Rate" column
    public decimal TaxPercentage { get; set; } // Table "Tax %" column
}