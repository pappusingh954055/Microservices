public class StockSummaryDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public string Unit { get; set; }
    public decimal MinStockLevel { get; set; }
    public decimal TotalReceived { get; set; }
    public decimal TotalRejected { get; set; }
    public decimal AvailableStock { get; set; }
    public decimal LastRate { get; set; }
    public int? LastPurchaseOrderId { get; set; }
    public int? LastSupplierId { get; set; }

    // Naya list history ke liye [cite: 2026-01-31]
    public List<StockHistoryDto> History { get; set; } = new List<StockHistoryDto>();
}

public class StockHistoryDto
{
    public DateTime ReceivedDate { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public decimal ReceivedQty { get; set; }
    public decimal RejectedQty { get; set; }

    public string ProductName { get; set; }
}
