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
            var existingUnits = (await _repo.GetAllAsync()).Select(u => u.Name.ToLower()).ToHashSet();
            var unitsToAdd = new List<UnitMaster>();

            foreach (var item in request.Units)
            {
                if (!string.IsNullOrWhiteSpace(item.Name) && !existingUnits.Contains(item.Name.ToLower()))
                {
                    var unit = new UnitMaster(item.Name, item.Description);
                    unitsToAdd.Add(unit);
                    existingUnits.Add(item.Name.ToLower()); // Prevent duplicates within the same batch
                }
            }

            if (unitsToAdd.Count == 0) return true; // Nothing to add, but not an error

            foreach (var unit in unitsToAdd)
            {
                await _repo.AddAsync(unit);
            }

            return await _uow.SaveChangesAsync(ct) > 0; 
        }
    }
}
