using MediatR;

namespace Inventory.Application.GatePasses.Queries.CheckDuplicateGatePass
{
    public record CheckDuplicateGatePassQuery(string ReferenceNo, string PassType) : IRequest<DuplicateGatePassResponse>;

    public record DuplicateGatePassResponse(bool IsDuplicate, string? PassNo, int? Status);
}
