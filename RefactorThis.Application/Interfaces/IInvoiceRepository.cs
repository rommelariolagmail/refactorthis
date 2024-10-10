using System.Threading.Tasks;
using System.Threading;
using RefactorThis.Domain.Entities;
using System;

namespace RefactorThis.Application.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<Invoice> GetInvoiceByReferenceAsync(string reference, CancellationToken cancellationToken);
        Task<bool> UpdateInvoiceAsync(Guid invoiceId, Invoice invoice);
    }
}
