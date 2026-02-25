using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inventory.Application.Units.Command
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
            var existingItems = await _repo.GetAllAsync();
            var existingUnits = existingItems.Select(u => u.Name.ToLower()).ToHashSet();
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

    public class UpdateUnitHandler : IRequestHandler<UpdateUnitCommand, bool>
    {
        private readonly IUnitRepository _repo;
        private readonly IUnitOfWork _uow;

        public UpdateUnitHandler(IUnitRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<bool> Handle(UpdateUnitCommand request, CancellationToken ct)
        {
            var unit = await _repo.GetByIdAsync(request.Id);
            if (unit == null) return false;

            unit.Update(request.Name, request.Description, request.IsActive);
            await _repo.UpdateAsync(unit);
            return await _uow.SaveChangesAsync(ct) > 0;
        }
    }

    public class DeleteUnitHandler : IRequestHandler<DeleteUnitCommand, bool>
    {
        private readonly IUnitRepository _repo;
        private readonly IUnitOfWork _uow;

        public DeleteUnitHandler(IUnitRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<bool> Handle(DeleteUnitCommand request, CancellationToken ct)
        {
            await _repo.DeleteAsync(request.Id);
            return await _uow.SaveChangesAsync(ct) > 0;
        }
    }
}
