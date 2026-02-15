using System;

namespace Suppliers.Domain.Entities
{
    public class SupplierPayment
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMode { get; set; } // Cash, Bank, etc.
        public string? ReferenceNumber { get; set; }
        public string? Remarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
