using Customers.Domain.Entities;
using System.Text.Json.Serialization;

namespace Customers.Application.DTOs
{
    public class CustomerLedgerResultDto
    {
        public string CustomerName { get; set; }
        public List<CustomerLedger> Ledger { get; set; }
    }

    public class CustomerLedgerRequestDto
    {
        public int CustomerId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SearchTerm { get; set; }
        public string? TypeFilter { get; set; }
        public string? ReferenceFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "TransactionDate";
        public string SortOrder { get; set; } = "desc";
    }

    public class CustomerLedgerPagedResultDto
    {
        public string CustomerName { get; set; }
        public decimal CurrentBalance { get; set; }
        public PaginatedListDto<CustomerLedger> Ledger { get; set; }
    }

    public class PaginatedListDto<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
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
        public string? LastReferenceId { get; set; }
    }

    public class OutstandingRequestDto
    {
        public string? SearchTerm { get; set; }
        public string? CustomerNameFilter { get; set; }
        public string? StatusFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CustomerName";
        public string SortOrder { get; set; } = "asc";
    }

    public class OutstandingPagedResultDto
    {
        public PaginatedListDto<OutstandingDto> Items { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
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

    public class MonthlyTrendDto
    {
        public string Month { get; set; }
        public decimal Amount { get; set; }
    }
}
