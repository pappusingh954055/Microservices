using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.GRN.DTOs.Stock
{
    public class StockSummaryDto
    {
        public Guid ProductId { get; set; }
        public int LastSupplierId { get; set; }
        public  int LastPurchaseOrderId { get; set; }
        public string? ProductName { get; set; }
        public decimal TotalReceived { get; set; } // Sum of ReceivedQty
        public string? Unit { get; set; }
        public decimal LastRate { get; set; } // Latest UnitRate from GRN
                                              // Logic: Agar stock 10 se kam hai toh alert
        public bool IsLowStock => TotalReceived < 10;

        public int MinStockLevel {  get; set; }

        public decimal AvailableStock { get; set; }
        public decimal TotalRejected { get; set; }
    }
}
