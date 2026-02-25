using Inventory.Application.Locations.Racks.DTOs;
using MediatR;

namespace Inventory.Application.Locations.Racks.Queries.GetRacks;

public record GetRacksQuery() : IRequest<List<RackDto>>;
