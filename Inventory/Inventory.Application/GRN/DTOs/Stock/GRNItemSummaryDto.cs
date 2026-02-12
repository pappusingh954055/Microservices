using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.GRN.DTOs.Stock
{
    public class GRNItemSummaryDto
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal OrderedQty { get; set; } // NAYA
        public decimal PendingQty { get; set; } // NAYA
        public decimal ReceivedQty { get; set; }
        public decimal RejectedQty { get; set; }
        public decimal UnitRate { get; set; }
    }
}
