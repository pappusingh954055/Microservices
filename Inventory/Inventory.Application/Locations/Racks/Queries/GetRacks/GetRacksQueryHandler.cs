using Inventory.Application.Common.Interfaces;
using Inventory.Application.Locations.Racks.DTOs;
using MediatR;

namespace Inventory.Application.Locations.Racks.Queries.GetRacks;

public sealed class GetRacksQueryHandler : IRequestHandler<GetRacksQuery, List<RackDto>>
{
    private readonly IRackRepository _repository;

    public GetRacksQueryHandler(IRackRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<RackDto>> Handle(GetRacksQuery request, CancellationToken cancellationToken)
    {
        var racks = await _repository.GetAllAsync();

        return racks.Select(r => new RackDto(
            r.Id,
            r.WarehouseId,
            r.Warehouse?.Name ?? "Unknown",
            r.Name,
            r.Description,
            r.IsActive
        )).ToList();
    }
}
