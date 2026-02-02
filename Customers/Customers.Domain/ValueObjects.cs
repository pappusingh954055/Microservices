using System;
using System.Collections.Generic;
using System.Text;

namespace Customers.Domain
{
    public class Address
    {
        public string AddressLine { get; private set; }

        private Address() { }

        public Address(string addressLine)
        {
            AddressLine = addressLine;
        }
    }
}
