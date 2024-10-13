using MediatR;
using RefactorThis.Domain.Entities;
using System;

namespace RefactorThis.Application.UseCases.InvoiceCRUD.Commands
{
    public sealed class DeleteInvoiceCommand : IRequest<Response<string>>
    {
        public Guid InvoiceId { get; set; }
    }
}