using MediatR;

namespace Customers.Application.Features.Commands
{
    public class DeleteCustomerCommand : IRequest<bool>
    {
        public int Id { get; }

        public DeleteCustomerCommand(int id)
        {
            Id = id;
        }
    }
}
