using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.GRN.DTOs.Stock
{
    public class StockSummaryDto
    {
        public string? ProductName { get; set; }
        public decimal TotalReceived { get; set; } // Sum of ReceivedQty
        public string? Unit { get; set; }
        public decimal LastRate { get; set; } // Latest UnitRate from GRN
                                              // Logic: Agar stock 10 se kam hai toh alert
        public bool IsLowStock => TotalReceived < 10;
    }
}
