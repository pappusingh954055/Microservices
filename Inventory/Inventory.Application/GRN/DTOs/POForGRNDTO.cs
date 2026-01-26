using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.GRN.DTOs
{
    public class POForGRNDTO
    {
        public int POHeaderId { get; set; }
        public string PONumber { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        //public decimal DiscountPercent { get; set; }
        public List<POItemForGRNDTO> Items { get; set; }
    }
}
