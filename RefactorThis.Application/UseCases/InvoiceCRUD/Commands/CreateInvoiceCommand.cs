using MediatR;
using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Enums;

namespace RefactorThis.Application.UseCases.InvoiceCRUD.Commands
{
    public sealed class CreateInvoiceCommand : IRequest<Response<Invoice>>
    {
        public decimal Amount { get; set; }
        public InvoiceType Type { get; set; }
    }
}