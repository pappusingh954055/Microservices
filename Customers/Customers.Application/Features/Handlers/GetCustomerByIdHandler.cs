using Customers.Application.Common.Interfaces;
using Customers.Application.DTOs;
using Customers.Application.Features.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Customers.Application.Features.Handlers
{
    public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
    {
        private readonly ICustomerRepository _repo;

        public GetCustomerByIdHandler(ICustomerRepository repo)
        {
            _repo = repo;
        }

        public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
        {
            var customer = await _repo.GetByIdAsync(request.Id);

            if (customer == null) return null;

            return new CustomerDto
            {
                Id = customer.Id,
                CustomerName = customer.CustomerName,
                CustomerType = customer.CustomerType,
                Phone = customer.Phone,
                Email = customer.Email,
                GstNumber = customer.GstNumber,
                CreditLimit = customer.CreditLimit,
                BillingAddress = customer.BillingAddress?.AddressLine,
                ShippingAddress = customer.ShippingAddress?.AddressLine,
                Status = customer.Status
            };
        }
    }
}
