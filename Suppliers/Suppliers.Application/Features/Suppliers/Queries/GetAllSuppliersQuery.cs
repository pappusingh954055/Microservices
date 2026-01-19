using MediatR;
using System.Collections.Generic;

public record GetAllSuppliersQuery() : IRequest<IEnumerable<SupplierDto>>;