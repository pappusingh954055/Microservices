using MediatR;
using Suppliers.Application.Common.Models;
using Suppliers.Application.DTOs;

namespace Suppliers.Application.Features.Suppliers.Queries;

public sealed record GetSuppliersPagedQuery(GridRequest Query) 
    : IRequest<GridResponse<SupplierDto>>;
