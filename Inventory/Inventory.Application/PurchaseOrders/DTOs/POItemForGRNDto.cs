using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.PurchaseOrders.DTOs
{
    public class POItemForGRNDto
    {
        public Guid ProductId { get; set; } // int se Guid kar diya
        public string ProductName { get; set; }
        public decimal OrderedQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal AlreadyReceivedQty { get; set; }
    }
}
