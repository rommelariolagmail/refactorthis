using MediatR;
using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Enums;
using System;
using System.Collections.Generic;

namespace RefactorThis.Application.UseCases.InvoiceCRUD.Commands
{
    public sealed class UpdateInvoiceCommand : IRequest<Response<string>>
    {
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public List<Payment> Payments { get; set; } = new List<Payment>();
        public InvoiceType Type { get; set; } = InvoiceType.Standard;
    }
}