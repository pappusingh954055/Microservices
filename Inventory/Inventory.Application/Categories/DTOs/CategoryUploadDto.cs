using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Categories.DTOs
{
    public class CategoryUploadDto
    {
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public decimal DefaultGst { get; set; }
        public string Description { get; set; }
    }
}
