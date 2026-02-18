using MediatR;
using Customers.Application.DTOs;
using System.Collections.Generic;

namespace Customers.Application.Features.Finance.Commands
{
    public record BulkRecordCustomerReceiptCommand(List<CustomerReceiptDto> Receipts) : IRequest<bool>;
}
