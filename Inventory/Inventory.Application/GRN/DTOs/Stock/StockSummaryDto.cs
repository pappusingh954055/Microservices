namespace Inventory.Application.GRN.DTOs.Stock.fix
{
    public class StockSummaryDto
    {
        public Guid ProductId { get; set; }
        public int LastSupplierId { get; set; }
        public int LastPurchaseOrderId { get; set; }
        public string? ProductName { get; set; }
        public decimal TotalReceived { get; set; }
        public string? Unit { get; set; }
        public decimal LastRate { get; set; }
        public bool IsLowStock => AvailableStock < 10; // Logic updated to AvailableStock
        public int MinStockLevel { get; set; }
        public decimal AvailableStock { get; set; }
        public decimal TotalRejected { get; set; }

        // Traceability history list
        public List<StockHistoryDto> History { get; set; } = new List<StockHistoryDto>();
    }

    // Ye class hona zaroori hai tabhi CS0234 error jayega
    public class StockHistoryDto
    {
        public DateTime ReceivedDate { get; set; }
        public string? PONumber { get; set; }
        public string? SupplierName { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal RejectedQty { get; set; }
    }
}