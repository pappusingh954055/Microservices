using MediatR;
using System.Collections.Generic;

namespace Suppliers.Application.Features.Suppliers.Queries
{
    public class GetSupplierBalancesQuery : IRequest<Dictionary<int, decimal>>
    {
        public List<int> SupplierIds { get; set; }

        public GetSupplierBalancesQuery(List<int> supplierIds)
        {
            SupplierIds = supplierIds;
        }
    }
}
