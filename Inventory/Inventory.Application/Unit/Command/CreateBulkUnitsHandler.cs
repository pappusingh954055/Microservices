using Inventory.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Application.Unit.Command
{
    public class CreateBulkUnitsHandler : IRequestHandler<CreateBulkUnitsCommand, bool>
    {
        private readonly IUnitRepository _repo;
        private readonly IUnitOfWork _uow; // Transaction handle karne ke liye

        public CreateBulkUnitsHandler(IUnitRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<bool> Handle(CreateBulkUnitsCommand request, CancellationToken ct)
        {
            foreach (var item in request.Units)
            {
                var unit = new UnitMaster(item.Name, item.Description); // DDD Entity
                await _repo.AddAsync(unit);
            }
            return await _uow.SaveChangesAsync(ct) > 0; 
        }
    }
}
