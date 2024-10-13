using MediatR;
using RefactorThis.Application.Interfaces;
using RefactorThis.Application.UseCases.InvoiceCRUD.Queries;
using RefactorThis.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorThis.Application.UseCases.InvoiceCRUD.Handlers
{
    public sealed class GetInvoiceByIdHandler : IRequestHandler<GetInvoiceByIdQuery, Response<Invoice>>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public GetInvoiceByIdHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<Response<Invoice>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
        {
            Response<Invoice> response = new Response<Invoice>();

            try
            {
                Invoice invoice = await _invoiceRepository.GetInvoiceByIdAsync(request.InvoiceId, cancellationToken);

                if (invoice is null)
                {
                    response.IsSuccess = false;
                    response.Messages.Add("Invoice not found.");
                    
                    return response;
                }

                response.Data.Add(invoice);

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
