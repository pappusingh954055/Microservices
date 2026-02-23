using Customers.Application.DTOs;
using MediatR;

namespace Customers.Application.Features.Commands
{
    public class UpdateCustomerCommand : IRequest<bool>
    {
        public int Id { get; }
        public CreateCustomerDto Dto { get; }

        public UpdateCustomerCommand(int id, CreateCustomerDto dto)
        {
            Id = id;
            Dto = dto;
        }
    }
}
