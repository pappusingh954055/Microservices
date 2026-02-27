using Customers.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Customers.Domain.Entities
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public string? CustomerName { get; private set; }
        public string? CustomerType { get; private set; }

        public string? Phone { get; private set; }
        public string? Email { get; private set; }

        public string? GstNumber { get; private set; }
        public decimal? CreditLimit { get; private set; }

        public Address? BillingAddress { get; private set; }
        public Address? ShippingAddress { get; private set; }

        public string? Status { get; private set; } = string.Empty;

        public string? CreatedBy { get; private set; }
        public string? UpdatedBy { get; private set; }
        public DateTime? CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; } = DateTime.UtcNow;

        // EF Core
        private Customer() { Status = null!; }



        public Customer(
            string customerName,
            string customerType,
            string phone,
            string? email,
            string? gstNumber,
            decimal creditLimit,
            Address billingAddress,
            Address? shippingAddress,
            string customerStatus,
            string createdBy)
        {
            
            CustomerName = customerName;
            CustomerType = customerType;
            Phone = phone;
            Email = email;
            GstNumber = gstNumber;
            CreditLimit = creditLimit;
            BillingAddress = billingAddress;
            ShippingAddress = shippingAddress;
            Status = customerStatus;
            CreatedBy = createdBy;
            CreatedAt = DateTime.UtcNow;
        }

        public void Update(
            string customerName,
            string customerType,
            string phone,
            string? email,
            string? gstNumber,
            decimal? creditLimit,
            Address billingAddress,
            Address? shippingAddress,
            string? status)
        {
            CustomerName = customerName;
            CustomerType = customerType;
            Phone = phone;
            Email = email;
            GstNumber = gstNumber;
            CreditLimit = creditLimit;
            BillingAddress = billingAddress;
            ShippingAddress = shippingAddress;
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(string status)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
        }

        public class Address
        {
            public string AddressLine { get; private set; }

            private Address() { }

            public Address(string address)
            {
                AddressLine = address;
            }
        }
    }
}
