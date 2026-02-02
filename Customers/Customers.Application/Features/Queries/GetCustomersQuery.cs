using Customers.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Customers.Application.Features.Queries
{
    public record GetCustomersQuery()
     : IRequest<List<CustomerDto>>;
}
