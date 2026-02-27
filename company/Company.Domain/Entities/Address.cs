using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Domain.Entities
{
    public class Address
    {
        public int Id { get; set; }
        public string AddressLine1 { get; set; } = string.Empty; // Shop/Office No, Building
        public string AddressLine2 { get; set; } = string.Empty; // Area, Landmark
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty; // GST rules ke liye (e.g., 07 for Delhi)
        public string PinCode { get; set; } = string.Empty;
        public string Country { get; set; } = "India";
        public string? Email { get; set; }
    }
}
