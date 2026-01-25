using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.PurchaseOrders.DTOs
{
    public class BulkDeleteRequestDto
    {
        public int PurchaseOrderId { get; set; }
        public List<int> ItemIds { get; set; } = new();
    }
}
