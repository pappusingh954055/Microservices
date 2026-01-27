using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.GRN.DTOs
{
    public class GRNListDto
    {
        public int Id { get; set; }
        public string GRNNo { get; set; }
        public string RefPO { get; set; }
        public string SupplierName { get; set; }
        public DateTime ReceivedDate { get; set; }
        public string Status { get; set; } // Completed or Partial
    }
    // Paged Response [cite: 2026-01-22]
    public class GRNPagedResponseDto
    {
        public List<GRNListDto> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
