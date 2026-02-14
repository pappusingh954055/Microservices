using Customers.Application.Common.Models;
using Customers.Application.DTOs;
using MediatR;

namespace Customers.Application.Features.Queries;

public sealed record GetCustomersPagedQuery(GridRequest Query) 
    : IRequest<GridResponse<CustomerDto>>;
