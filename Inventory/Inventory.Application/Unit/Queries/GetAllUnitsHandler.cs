using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Unit.Queries
{
    public class GetAllUnitsHandler : IRequestHandler<GetAllUnitsQuery, IEnumerable<UnitDto>>
    {
        private readonly IUnitRepository _repo;
        public GetAllUnitsHandler(IUnitRepository repo) => _repo = repo;

        public async Task<IEnumerable<UnitDto>> Handle(GetAllUnitsQuery request, CancellationToken ct)
        {
            var units = await _repo.GetAllAsync();
            return units.Select(u => new UnitDto(u.Id, u.Name, u.Description, u.IsActive));
        }
    }
}
