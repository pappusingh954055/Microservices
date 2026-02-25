using MediatR;

namespace Inventory.Application.Locations.Racks.Commands.DeleteRack;

public record DeleteRackCommand(Guid Id) : IRequest<MediatR.Unit>;
