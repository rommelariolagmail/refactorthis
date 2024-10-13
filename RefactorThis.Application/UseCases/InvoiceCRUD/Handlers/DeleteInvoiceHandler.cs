using MediatR;
using RefactorThis.Application.Interfaces;
using RefactorThis.Application.UseCases.InvoiceCRUD.Commands;
using RefactorThis.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorThis.Application.UseCases.InvoiceCRUD.Handlers
{
    public sealed class DeleteInvoiceHandler : IRequestHandler<DeleteInvoiceCommand, Response<string>>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public DeleteInvoiceHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<Response<string>> Handle(DeleteInvoiceCommand request, CancellationToken cancellationToken)
        {
            Response<string> response = new Response<string>();

            try
            {
                Invoice invoice = await _invoiceRepository.GetInvoiceByIdAsync(request.InvoiceId, cancellationToken);

                if (invoice is null)
                {
                    response.IsSuccess = false;
                    response.Messages.Add("Invoice not found.");

                    return response;
                }

                await _invoiceRepository.DeleteInvoiceAsync(request.InvoiceId);

                response.Data.Add("Invoice deleted.");

                return response;
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return response;
        }
    }
}
