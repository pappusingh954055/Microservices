using MediatR;
using Customers.Application.Common.Interfaces;
using Customers.Application.Features.Finance.Commands;
using Customers.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Customers.Application.Features.Finance.Handlers
{
    public class BulkRecordCustomerReceiptHandler : IRequestHandler<BulkRecordCustomerReceiptCommand, bool>
    {
        private readonly IFinanceRepository _repository;

        public BulkRecordCustomerReceiptHandler(IFinanceRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(BulkRecordCustomerReceiptCommand request, CancellationToken cancellationToken)
        {
            if (request.Receipts == null || !request.Receipts.Any())
                return false;

            // Group receipts by customer to process sequentially per customer
            var receiptsByCustomer = request.Receipts.GroupBy(r => r.CustomerId);

            foreach (var customerGroup in receiptsByCustomer)
            {
                int customerId = customerGroup.Key;
                
                // Get initial balance from DB
                var lastLedger = await _repository.GetLastLedgerEntryAsync(customerId);
                decimal currentBalance = lastLedger?.Balance ?? 0;

                foreach (var receiptDto in customerGroup)
                {
                    // Create Receipt Entity
                    var customerReceipt = new CustomerReceipt
                    {
                        CustomerId = customerId,
                        Amount = receiptDto.Amount,
                        ReceiptDate = receiptDto.ReceiptDate,
                        ReceiptMode = receiptDto.ReceiptMode,
                        ReferenceNumber = receiptDto.ReferenceNumber,
                        Remarks = receiptDto.Remarks,
                        CreatedBy = receiptDto.CreatedBy ?? "System",
                        CreatedDate = System.DateTime.Now
                    };

                    await _repository.AddReceiptAsync(customerReceipt);

                    // Update local running balance
                    currentBalance -= receiptDto.Amount;

                    // Create Ledger Entry
                    var ledgerEntry = new CustomerLedger
                    {
                        CustomerId = customerId,
                        TransactionType = "Receipt",
                        ReferenceId = string.IsNullOrWhiteSpace(receiptDto.ReferenceNumber) 
                            ? "REC-" + System.Guid.NewGuid().ToString().Substring(0, 8) 
                            : receiptDto.ReferenceNumber,
                        Debit = 0,
                        Credit = receiptDto.Amount,
                        Balance = currentBalance,
                        TransactionDate = receiptDto.ReceiptDate,
                        Description = "Receipt Received: " + receiptDto.ReceiptMode,
                        CreatedBy = receiptDto.CreatedBy ?? "System",
                        CreatedDate = System.DateTime.Now
                    };

                    await _repository.AddLedgerEntryAsync(ledgerEntry);
                }
            }

            await _repository.SaveChangesAsync();
            return true;
        }
    }
}
