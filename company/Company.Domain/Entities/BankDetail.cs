using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Domain.Entities
{
    public class BankDetail
    {
        public int Id { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IfscCode { get; set; } = string.Empty;
        public string AccountType { get; set; } = "Current"; // Savings/Current
        public string? Email { get; set; }
    }
}
