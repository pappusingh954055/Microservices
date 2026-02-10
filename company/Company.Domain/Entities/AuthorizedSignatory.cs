using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Domain.Entities
{
    public class AuthorizedSignatory
    {
        public int Id { get; set; }
        public int CompanyProfileId { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string? SignatureImageUrl { get; set; } // Digital Signature optional
        public bool IsDefault { get; set; } = true;

    }
}
