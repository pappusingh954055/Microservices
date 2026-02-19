using Inventory.Application.Common.Interfaces;
using Inventory.Application.GatePasses.DTOs;
using Inventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.GatePasses.Commands.CreateGatePass
{
    public class CreateGatePassCommandHandler : IRequestHandler<CreateGatePassCommand, GatePassDto>
    {
        private readonly IInventoryDbContext _context;

        public CreateGatePassCommandHandler(IInventoryDbContext context)
        {
            _context = context;
        }

        public async Task<GatePassDto> Handle(CreateGatePassCommand request, CancellationToken cancellationToken)
        {
            var year = DateTime.UtcNow.Year;
            var isInward = request.PassType == "Inward";
            var prefix = isInward ? "GP-IN" : "GP";

            // Count existing for this year and type to generate sequence
            var count = await _context.GatePasses
                .Where(x => x.CreatedAt.HasValue && x.CreatedAt.Value.Year == year && x.PassType == request.PassType)
                .CountAsync(cancellationToken);

            var passNo = $"{prefix}-{year}-{(count + 1).ToString("D4")}";

            var entity = new GatePass
            {
                PassNo = passNo,
                PassType = request.PassType,
                ReferenceType = request.ReferenceType,
                ReferenceId = request.ReferenceId,
                ReferenceNo = request.ReferenceNo,
                InvoiceNo = request.InvoiceNo,
                PartyName = request.PartyName,
                VehicleNo = request.VehicleNo,
                VehicleType = request.VehicleType,
                DriverName = request.DriverName,
                DriverPhone = request.DriverPhone,
                TransporterName = request.TransporterName,
                TotalQty = request.TotalQty,
                TotalWeight = request.TotalWeight,
                GateEntryTime = request.GateEntryTime,
                SecurityGuard = request.SecurityGuard,
                Status = request.Status, // 1 = Entered/Created
                Remarks = request.Remarks,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.GatePasses.Add(entity);

            // --- NEW: Update Reference Table with GatePassNo ---
            if (request.ReferenceType == 3 && int.TryParse(request.ReferenceId, out int soId)) // 3 = SaleOrder
            {
                var saleOrder = await _context.SaleOrders.FirstOrDefaultAsync(s => s.Id == soId, cancellationToken);
                if (saleOrder != null)
                {
                    saleOrder.GatePassNo = entity.PassNo;
                }
            }
            else if (request.ReferenceType == 1 && int.TryParse(request.ReferenceId, out int poId)) // 1 = PurchaseOrder
            {
                var purchaseOrder = await _context.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == poId, cancellationToken);
                if (purchaseOrder != null)
                {
                    // Update if property exists (assuming we added it)
                    // purchaseOrder.GatePassNo = entity.PassNo; 
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new GatePassDto
            {
                Id = entity.Id,
                PassNo = entity.PassNo,
                PassType = entity.PassType
            };
        }
    }
}
