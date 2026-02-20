using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Unit.Queries
{
    public record GetAllUnitsQuery() : IRequest<IEnumerable<UnitDto>>;
}
