using MediatR;
using Suppliers.Application.DTOs;
using Suppliers.Application.Features.Suppliers.Commands;
using Suppliers.Application.Interfaces;
using Suppliers.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Suppliers.Application.Features.Suppliers.Handlers
{
    public class RecordSupplierPurchaseHandler(IFinanceRepository repository) : IRequestHandler<RecordSupplierPurchaseCommand, bool>
    {
        private readonly IFinanceRepository _repository = repository;

        public async Task<bool> Handle(RecordSupplierPurchaseCommand request, CancellationToken cancellationToken)
        {
            var dto = request.PurchaseData;
            var isDebitNote = dto.TransactionType == "DebitNote"; // Check if it's a return

            // Get last balance to calculate new balance
            var lastLedger = await _repository.GetLastLedgerEntryAsync(dto.SupplierId);
            
            // For Supplier: 
            // Purchase (Credit) increases Balance (we owe them more)
            // Payment/DebitNote (Debit) decreases Balance (we owe them less)
            decimal currentBalance = (lastLedger?.Balance ?? 0) + (isDebitNote ? -dto.Amount : dto.Amount);

            var supplierLedger = new SupplierLedger
            {
                SupplierId = dto.SupplierId,
                TransactionType = isDebitNote ? "Debit Note" : "Purchase",
                ReferenceId = dto.ReferenceId,
                Debit = isDebitNote ? dto.Amount : 0,
                Credit = isDebitNote ? 0 : dto.Amount,
                Balance = currentBalance,
                TransactionDate = dto.TransactionDate,
                Description = dto.Description ?? (isDebitNote ? "Purchase Return: " : "Purchase via GRN: ") + dto.ReferenceId
            };

            await _repository.AddLedgerEntryAsync(supplierLedger);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
