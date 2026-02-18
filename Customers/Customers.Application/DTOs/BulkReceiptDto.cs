using System;
using System.Collections.Generic;

namespace Customers.Application.DTOs
{
    public class BulkReceiptDto
    {
        public List<CustomerReceiptDto> Receipts { get; set; } = new List<CustomerReceiptDto>();
    }
}
