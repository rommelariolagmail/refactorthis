using MediatR;
using RefactorThis.Application.Interfaces;
using RefactorThis.Application.UseCases.Shared;
using RefactorThis.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorThis.Application.UseCases.ProcessPayment
{
    public sealed class ProcessPaymentHandler : IRequestHandler<ProcessPaymentCommand, Response<string>>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public ProcessPaymentHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<Response<string>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
        {
            Response<string> response = new Response<string>();

            try
            {
                #region request validation
                ValidateRequest(request, ref response);

                if (response.Messages.Any())
                {
                    response.IsSuccess = false;
                    
                    return response;
                }
                #endregion

                #region invoice validation
                Invoice invoice = await _invoiceRepository.GetInvoiceByReferenceAsync(
                    request.Reference,
                    cancellationToken);

                ValidateInvoiceRequest(invoice, ref response);

                if (response.Messages.Any())
                {
                    response.IsSuccess = false;

                    return response;
                }
                #endregion

                #region payment validation
                if (request.Amount + invoice.AmountPaid > invoice.Amount + invoice.TaxAmount)
                {
                    response.IsSuccess = false;
                    response.Messages.Add("Over payment.");

                    return response;
                }
                #endregion

                invoice.Payments.Add(new Payment
                {
                    Amount = request.Amount,
                    Reference = request.Reference,
                });
                invoice.AmountPaid = invoice.Payments.Sum(x => x.Amount);

                _ = await _invoiceRepository.UpdateInvoiceAsync(invoice);

                string paymentStatus = invoice.Amount + invoice.TaxAmount == invoice.AmountPaid
                    ? "fully" : "partially";

                response.Messages.Add($"Invoice is now {paymentStatus} paid.");
            }
            catch (Exception ex)
            {
                response.SetException(ex);
            }

            return response;
        }

        private void ValidateRequest(ProcessPaymentCommand request, ref Response<string> response)
        {
            if (string.IsNullOrEmpty(request.Reference))
            {
                response.Messages.Add("Missing payment reference.");
            }

            if (request.Amount <= 0)
            {
                response.Messages.Add("Invalid payment amount.");
            }
        }

        private static void ValidateInvoiceRequest(Invoice invoice, ref Response<string> response)
        {
            if (invoice is null)
            {
                response.Messages.Add("There is no invoice matching this payment");

                return;
            }

            if (invoice.Amount + invoice.TaxAmount == 0)
            {
                if (invoice.Payments?.Any() == false)
                {
                    response.Messages.Add("No payment needed.");
                }
            }

            if (invoice.Amount + invoice.TaxAmount == invoice.AmountPaid)
            {
                response.Messages.Add("The invoice is already fully paid.");
            }

            InvoiceValidation.ValidateInvoice(invoice, ref response);
        }
    }
}