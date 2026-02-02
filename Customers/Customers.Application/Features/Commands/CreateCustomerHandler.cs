using Customers.Application.Common.Interfaces;
using Customers.Application.Features.Commands;
using Customers.Domain.Entities;
using MediatR;
using static Customers.Domain.Entities.Customer;

namespace Customers.Application.Features.Handlers;

public class CreateCustomerHandler
    : IRequestHandler<CreateCustomerCommand, int>
{
    private readonly ICustomerRepository _repo;

    public CreateCustomerHandler(ICustomerRepository repo)
    {
        _repo = repo;
    }

    public async Task<int> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        var customer = new Customer(
            dto.CustomerName,
            dto.CustomerType,
            dto.Phone,
            dto.Email,
            dto.GstNumber,
            dto.CreditLimit,
            new Address(dto.BillingAddress),
            string.IsNullOrWhiteSpace(dto.ShippingAddress)
                ? null
                : new Address(dto.ShippingAddress),
            dto.CustomerStatus,
            dto.CreatedBy
        );

        await _repo.AddAsync(customer);

        return customer.Id;   // INT
    }
}
