using Inventory.Domain.PriceLists;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.PurchaseOrders.DTOs
{
    public class UpdatePurchaseOrderDto
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string PoNumber { get; set; }
        public int PriceListId { get; set; }
        public PriceList PriceList { get; set; }
        public DateTime PoDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public decimal TotalTax { get; set; }
        public decimal GrandTotal { get; set; }
        public List<UpdatePurchaseOrderItemDto> Items { get; set; }
    }
}
