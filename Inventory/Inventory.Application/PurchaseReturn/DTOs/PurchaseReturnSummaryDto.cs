using System;

namespace Inventory.Application.PurchaseReturn.DTOs
{
    public class PurchaseReturnSummaryDto
    {
        public int TotalReturnsToday { get; set; }
        public decimal TotalRefundValue { get; set; } // GrandTotal of confirmed returns
        public decimal StockReducedPcs { get; set; } // Sum of ReturnQty
        public int ConfirmedReturns { get; set; }
        public int PendingOutwardCount { get; set; } // Confirmed but no GatePassNo
    }
}
