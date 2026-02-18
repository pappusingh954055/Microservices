using System;

namespace Inventory.Domain.Entities
{
    public class GatePass
    {
        public int Id { get; set; }
        public string PassNo { get; set; }
        public string PassType { get; set; } // Inward / Outward
        public int ReferenceType { get; set; } // 1=PO, 2=GRN...
        public int ReferenceId { get; set; }
        public string ReferenceNo { get; set; }
        public string? InvoiceNo { get; set; } // For Inward
        public string PartyName { get; set; }
        public string VehicleNo { get; set; }
        public string? VehicleType { get; set; }
        public string DriverName { get; set; }
        public string DriverPhone { get; set; }
        public string? TransporterName { get; set; }
        public decimal TotalQty { get; set; }
        public decimal? TotalWeight { get; set; }
        public DateTime GateEntryTime { get; set; }
        public string SecurityGuard { get; set; }
        public int Status { get; set; } // 1=Entered...
        public string? Remarks { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
