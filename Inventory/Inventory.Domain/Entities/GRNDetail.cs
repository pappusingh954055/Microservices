using Inventory.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain.Entities
{
    public class GRNDetail: BaseAuditableEntity
    {
        public int Id { get; set; }
        public int GRNHeaderId { get; set; }
        public GRNHeader GRNHeader { get; set; }
        public Guid ProductId { get; set; }    
        public Product Product { get; set; }
        public decimal OrderedQty { get; set; }
        public decimal PendingQty { get; set; }
        public decimal RejectedQty { get; set; }
        public decimal AcceptedQty { get; set; }
        public decimal ReceivedQty { get; set; } // User input quantity [cite: 2026-01-22]
        public decimal UnitRate { get; set; }
    }
}
