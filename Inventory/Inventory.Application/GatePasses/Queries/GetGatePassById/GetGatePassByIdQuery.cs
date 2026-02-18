using Inventory.Application.GatePasses.DTOs;
using MediatR;

namespace Inventory.Application.GatePasses.Queries.GetGatePassById
{
    public record GetGatePassByIdQuery(int Id) : IRequest<GatePassDto?>;
}
