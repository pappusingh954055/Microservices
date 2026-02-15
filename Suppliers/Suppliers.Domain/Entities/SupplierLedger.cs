using System;

namespace Suppliers.Domain.Entities
{
    public class SupplierLedger
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string TransactionType { get; set; } // Purchase, Return, Payment
        public string ReferenceId { get; set; } // Bill No or Payment Id
        public decimal Debit { get; set; } // Payments/Returns
        public decimal Credit { get; set; } // Purchases
        public decimal Balance { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
