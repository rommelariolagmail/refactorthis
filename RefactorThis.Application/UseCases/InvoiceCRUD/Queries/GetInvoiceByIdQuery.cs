using MediatR;
using RefactorThis.Domain.Entities;
using System;

namespace RefactorThis.Application.UseCases.InvoiceCRUD.Queries
{
    public sealed class GetInvoiceByIdQuery : IRequest<Response<Invoice>>
    {
        public Guid InvoiceId { get; set; }
    }
}