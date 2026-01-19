using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.PriceLists.DTOs
{
    public sealed class CreatePriceListDto
    {
        // Id ki zarurat Create ke time nahi hai, Handler generate karega
        public string name { get; set; } = string.Empty;
        public string code { get; set; } = string.Empty;
        public string? priceType { get; set; } // SALES ya PURCHASE
        public DateTime? validFrom { get; set; }
        public DateTime? validTo { get; set; }
        public bool isActive { get; set; }
        public string? description { get; set; }

        // Audit fields (Ye usually BaseEntity ya Handler handle karta hai)
        public int? createdBy { get; set; }

        // Child Items List
        public List<CreatePriceListItemDto> Items { get; set; } = new();
    }

    public sealed class CreatePriceListItemDto
    {
        public Guid productId { get; set; } // Angular se aane wala GUID
        public decimal price { get; set; }
        public int minQty { get; set; }
        public int? maxQty { get; set; }
        public bool isActive { get; set; }
    }
}
