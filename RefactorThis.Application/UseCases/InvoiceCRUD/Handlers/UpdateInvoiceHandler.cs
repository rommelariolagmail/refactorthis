using MediatR;
using RefactorThis.Application.Interfaces;
using RefactorThis.Application.UseCases.InvoiceCRUD.Commands;
using RefactorThis.Application.UseCases.Shared;
using RefactorThis.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorThis.Application.UseCases.InvoiceCRUD.Handlers
{
    public sealed class UpdateInvoiceHandler : IRequestHandler<UpdateInvoiceCommand, Response<string>>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public UpdateInvoiceHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<Response<string>> Handle(UpdateInvoiceCommand request, CancellationToken cancellationToken)
        {
            Response<string> response = new Response<string>();

            try
            {
                #region request validation
                Invoice invoice = await _invoiceRepository.GetInvoiceByIdAsync(request.InvoiceId, cancellationToken);

                if (invoice is null)
                {
                    response.IsSuccess = false;
                    response.Messages.Add("Invoice not found.");

                    return response;
                }

                Invoice tempInvoice = new Invoice
                {
                    Amount = request.Amount != invoice.Amount ? request.Amount : invoice.Amount,
                    Payments = request.Payments?.Sum(p => p.Amount) != invoice.Payments?.Sum(p => p.Amount)
                        ? request.Payments : invoice.Payments,
                    Type = request.Type != invoice.Type ? request.Type : invoice.Type,
                };

                tempInvoice.AmountPaid = tempInvoice.Payments?.Sum(p => p.Amount) ?? 0;
                tempInvoice.TaxAmount = invoice.Type == Domain.Enums.InvoiceType.Commercial
                    ? tempInvoice.Amount * 0.1M : 0;

                InvoiceValidation.ValidateInvoice(tempInvoice, ref response);

                if (response.Messages.Any())
                {
                    response.IsSuccess = false;

                    return response;
                }
                #endregion

                await _invoiceRepository.UpdateInvoiceAsync(invoice);

                response.Messages.Add("Invoice updated.");

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