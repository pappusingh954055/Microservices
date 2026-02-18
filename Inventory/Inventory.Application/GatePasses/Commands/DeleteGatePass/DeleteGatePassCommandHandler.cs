using Inventory.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.GatePasses.Commands.DeleteGatePass
{
    public class DeleteGatePassCommandHandler : IRequestHandler<DeleteGatePassCommand, bool>
    {
        private readonly IInventoryDbContext _context;

        public DeleteGatePassCommandHandler(IInventoryDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteGatePassCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.GatePasses.FindAsync(new object[] { request.Id }, cancellationToken);
            
            if (entity == null) return false;

            _context.GatePasses.Remove(entity);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
