using System;

namespace Customers.Domain.Entities
{
    public class CustomerReceipt
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string ReceiptMode { get; set; } // GPay, Cash, Check, etc.
        public string? ReferenceNumber { get; set; }
        public string? Remarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
