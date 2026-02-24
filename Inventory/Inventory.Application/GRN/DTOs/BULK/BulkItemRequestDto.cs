using System;

namespace Inventory.Application.GRN.DTOs.BULK
{
    public class BulkItemRequestDto
    {
        public int POId { get; set; }
        public Guid ProductId { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal RejectedQty { get; set; }
        public decimal UnitRate { get; set; }
    }
}
