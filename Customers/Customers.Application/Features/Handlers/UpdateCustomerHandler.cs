using Customers.Application.Common.Interfaces;
using Customers.Application.Features.Commands;
using MediatR;
using static Customers.Domain.Entities.Customer;

namespace Customers.Application.Features.Handlers
{
    public class UpdateCustomerHandler : IRequestHandler<UpdateCustomerCommand, bool>
    {
        private readonly ICustomerRepository _repo;

        public UpdateCustomerHandler(ICustomerRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _repo.GetByIdAsync(request.Id);

            if (customer == null) return false;

            var dto = request.Dto;

            customer.Update(
                dto.CustomerName,
                dto.CustomerType,
                dto.Phone,
                dto.Email,
                dto.GstNumber,
                dto.CreditLimit,
                new Address(dto.BillingAddress),
                string.IsNullOrWhiteSpace(dto.ShippingAddress) ? null : new Address(dto.ShippingAddress),
                dto.CustomerStatus
            );

            await _repo.UpdateAsync(customer);

            return true;
        }
    }
}
