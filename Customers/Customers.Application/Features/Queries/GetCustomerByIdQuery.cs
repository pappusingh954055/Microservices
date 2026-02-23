using Customers.Application.DTOs;
using MediatR;

namespace Customers.Application.Features.Queries
{
    public class GetCustomerByIdQuery : IRequest<CustomerDto>
    {
        public int Id { get; }

        public GetCustomerByIdQuery(int id)
        {
            Id = id;
        }
    }
}
