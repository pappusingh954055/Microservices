using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.GRN.DTOs
{
    public class SaveGRNCommandDTO
    {
        public int POHeaderId { get; set; }
        public int SupplierId { get; set; }
        public DateTime ReceivedDate { get; set; }
        public string Remarks { get; set; }
        public decimal TotalAmount { get; set; }
        public string CreatedBy { get; set; }   
        public List<GRNItemDTO> Items { get; set; }
    }
}
