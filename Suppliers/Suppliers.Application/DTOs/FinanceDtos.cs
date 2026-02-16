using Suppliers.Domain.Entities;
using System.Collections.Generic;

namespace Suppliers.Application.DTOs
{
    public class SupplierLedgerResultDto
    {
        public string SupplierName { get; set; }
        public List<SupplierLedger> Ledger { get; set; }
    }

    public class PendingDueDto
    {
        public int SupplierId { get; set; }
        public decimal PendingAmount { get; set; }
        public string SupplierName { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class DateRangeDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
