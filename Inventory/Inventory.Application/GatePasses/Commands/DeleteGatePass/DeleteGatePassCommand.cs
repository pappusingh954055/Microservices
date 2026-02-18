using MediatR;

namespace Inventory.Application.GatePasses.Commands.DeleteGatePass
{
    public record DeleteGatePassCommand(int Id) : IRequest<bool>;
}
