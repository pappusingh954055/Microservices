using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Products.DTOs
{
    public class ProductSearchResponseDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public bool? IsActive { get; set; }    
        public decimal BasePurchasePrice { get; set; }
    }
}
