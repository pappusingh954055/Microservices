using Inventory.Application.Common.Interfaces;
using Inventory.Application.GRN.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.GRN.Queries
{
    public class GetPOForGRNHandler : IRequestHandler<GetPOForGRNQuery, POForGRNDTO>
    {
        private readonly IGRNRepository _repo;
        public GetPOForGRNHandler(IGRNRepository repo) => _repo = repo;

        public async Task<POForGRNDTO> Handle(GetPOForGRNQuery request, CancellationToken ct)
        {
            return await _repo.GetPODataForGRN(request.POId);
        }
    }
}
