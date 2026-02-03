using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.PurchaseReturn.DTOs
{
    public class PurchaseReturnDto
    {
        public int SupplierId { get; set; } // Changed to int [cite: 2026-02-03]
        public DateTime ReturnDate { get; set; }
        public string Remarks { get; set; }
        public List<PurchaseReturnItemDto> Items { get; set; }
    }

    public class PurchaseReturnItemDto
    {
        public Guid ProductId { get; set; } // Guid [cite: 2026-02-03]
        public string GrnRef { get; set; }
        public decimal ReturnQty { get; set; }
        public decimal Rate { get; set; }
    }
}
