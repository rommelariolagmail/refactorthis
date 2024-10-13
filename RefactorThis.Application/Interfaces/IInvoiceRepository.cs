using System.Threading.Tasks;
using System.Threading;
using RefactorThis.Domain.Entities;
using System;

namespace RefactorThis.Application.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<Invoice> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken);
        Task<Invoice> GetInvoiceByReferenceAsync(string reference, CancellationToken cancellationToken);
        Task<bool> CreateInvoiceAsync(Invoice invoice);
        Task<bool> UpdateInvoiceAsync(Invoice invoice);
        Task<bool> DeleteInvoiceAsync(Guid invoiceId);
    }
}
