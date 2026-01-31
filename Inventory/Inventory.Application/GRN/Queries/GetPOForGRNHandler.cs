using Inventory.Application.Common.Interfaces;
using Inventory.Application.GRN.DTOs;
using MediatR;

namespace Inventory.Application.GRN.Queries
{
    public class GetPOForGRNHandler : IRequestHandler<GetPOForGRNQuery, POForGRNDTO?>
    {
        private readonly IGRNRepository _repo;
        public GetPOForGRNHandler(IGRNRepository repo) => _repo = repo;

        public async Task<POForGRNDTO?> Handle(GetPOForGRNQuery request, CancellationToken ct)
        {
            // Logic Fix: Dono parameters pass karein taaki View mode chale
            return await _repo.GetPODataForGRN(request.POId, request.GrnHeaderId);
        }
    }
}