using Customers.Domain.Entities;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Customers.Application.DTOs
{
    public class CustomerLedgerResultDto
    {
        public string CustomerName { get; set; }
        public List<CustomerLedger> Ledger { get; set; }
    }

    public class CustomerReceiptDto
    {
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }

        [JsonPropertyName("paymentDate")]
        public DateTime ReceiptDate { get; set; }

        [JsonPropertyName("paymentMode")]
        public string ReceiptMode { get; set; } // Cash, Bank, etc.
        public string? ReferenceNumber { get; set; }
        public string? Remarks { get; set; }
        public string CreatedBy { get; set; }
    }

    public class OutstandingDto
    {
        public int CustomerId { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
    }

    public class DateRangeDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class CustomerSaleDto
    {
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceId { get; set; } // Invoice/Sale Order Number
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
