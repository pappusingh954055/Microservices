using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Company.Domain.Entities
{
    public class CompanyProfile
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Aapki Company ka Naam
        public string Tagline { get; set; } = string.Empty; // Optional slogan
        public string RegistrationNumber { get; set; } = string.Empty; // PAN/VAT No.
        public string Gstin { get; set; } = string.Empty; // Tax ke liye sabse zaroori
        public string LogoUrl { get; set; } = string.Empty; // Report ke header ke liye
        public string PrimaryEmail { get; set; } = string.Empty;
        public string PrimaryPhone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Foreign Keys
        public int AddressId { get; set; }
        public virtual Address CompanyAddress { get; set; }

        public int BankDetailId { get; set; }
        public virtual BankDetail BankInformation { get; set; }

        // Authorized Signatories
        public virtual ICollection<AuthorizedSignatory> AuthorizedSignatories { get; set; } = new List<AuthorizedSignatory>();
    }
}

