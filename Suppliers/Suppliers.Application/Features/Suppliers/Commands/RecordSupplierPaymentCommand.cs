using MediatR;
using Suppliers.Application.DTOs;

namespace Suppliers.Application.Features.Suppliers.Commands
{
    public record RecordSupplierPaymentCommand(SupplierPaymentDto PaymentData) : IRequest<int>;
}
