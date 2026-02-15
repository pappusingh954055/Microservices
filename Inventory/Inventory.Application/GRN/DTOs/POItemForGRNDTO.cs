using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.GRN.DTOs
{
    public class POItemForGRNDTO
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal OrderedQty { get; set; }
        public decimal PendingQty { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal GstPercent { get; set; }
        public decimal UnitRate { get; set; }

        public decimal ReceivedQty { get; set; }
        public decimal RejectedQty { get; set; }
        public decimal AcceptedQty { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
