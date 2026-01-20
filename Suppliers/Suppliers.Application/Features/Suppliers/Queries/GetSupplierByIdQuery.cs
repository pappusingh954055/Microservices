using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Suppliers.Application.Features.Suppliers.Queries
{
    public record GetSupplierByIdQuery(int Id) : IRequest<SupplierDto?>;
}
