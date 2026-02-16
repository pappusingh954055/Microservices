using MediatR;
using Customers.Application.DTOs;
using Customers.Application.Common.Interfaces;
using Customers.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Customers.Application.Features.Finance.Commands;

namespace Customers.Application.Features.Finance.Handlers
{
    public class RecordCustomerSaleHandler : IRequestHandler<RecordCustomerSaleCommand, int>
    {
        private readonly IFinanceRepository _financeRepository;

        public RecordCustomerSaleHandler(IFinanceRepository financeRepository)
        {
            _financeRepository = financeRepository;
        }

        public async Task<int> Handle(RecordCustomerSaleCommand request, CancellationToken cancellationToken)
        {
            var sale = request.SaleDto;

            // 1. Get last balance
            var lastEntry = await _financeRepository.GetLastLedgerEntryAsync(sale.CustomerId);
            decimal currentBalance = lastEntry?.Balance ?? 0;

            // 2. New balance (Debit increase for Sale)
            decimal newBalance = currentBalance + sale.Amount;

            // 3. Create Ledger Entry
            var entry = new CustomerLedger
            {
                CustomerId = sale.CustomerId,
                TransactionDate = sale.TransactionDate,
                TransactionType = "Sale",
                ReferenceId = sale.ReferenceId,
                Description = sale.Description,
                Debit = sale.Amount,
                Credit = 0,
                Balance = newBalance,
                CreatedBy = sale.CreatedBy,
                CreatedDate = DateTime.Now
            };

            await _financeRepository.AddLedgerEntryAsync(entry);
            await _financeRepository.SaveChangesAsync();

            return entry.Id;
        }
    }
}
