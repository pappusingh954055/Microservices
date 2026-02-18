using Inventory.Application.Common.Interfaces;
using Inventory.Application.GatePasses.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.GatePasses.Queries.GetGatePassById
{
    public class GetGatePassByIdHandler : IRequestHandler<GetGatePassByIdQuery, GatePassDto?>
    {
        private readonly IInventoryDbContext _context;

        public GetGatePassByIdHandler(IInventoryDbContext context)
        {
            _context = context;
        }

        public async Task<GatePassDto?> Handle(GetGatePassByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _context.GatePasses
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            
            return entity != null ? GatePassDto.FromEntity(entity) : null;
        }
    }
}
