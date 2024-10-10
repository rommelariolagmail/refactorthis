using MediatR;
using RefactorThis.Application.Interfaces;
using RefactorThis.Domain.Entities;
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

            #region request validation
            ValidateRequest(request, ref response);

            if (!response.IsSuccess)
            {
                return response;
            }
            #endregion

            #region invoice validation
            Invoice invoice = await _invoiceRepository.GetInvoiceByReferenceAsync(
                request.Reference,
                cancellationToken);

            ValidateInvoice(invoice, ref response);

            if (!response.IsSuccess)
            {
                return response;
            }
            #endregion

            #region payment validation
            if (request.Amount + invoice.AmountPaid > invoice.Amount + invoice.TaxAmount)
            {
                response.IsSuccess = false;
                response.Data.Add("Over payment.");
            }

            if (!response.IsSuccess)
            {
                return response;
            }
            #endregion

            invoice.Payments.Add(new Payment
            {
                Amount = request.Amount,
                Reference = request.Reference,
            });
            invoice.AmountPaid = invoice.Payments.Sum(x => x.Amount);

            _ = await _invoiceRepository.UpdateInvoiceAsync(invoice.Id, invoice);

            string paymentStatus = invoice.Amount + invoice.TaxAmount == invoice.AmountPaid
                ? "fully" : "partially";

            response.Data.Add($"Invoice is now {paymentStatus} paid.");

            return response;
        }

        private void ValidateRequest(ProcessPaymentCommand request, ref Response<string> response)
        {
            if (string.IsNullOrEmpty(request.Reference))
            {
                response.IsSuccess = false;
                response.Data.Add("Missing payment reference.");
            }

            if (request.Amount <= 0)
            {
                response.IsSuccess = false;
                response.Data.Add("Invalid payment amount.");
            }
        }

        private void ValidateInvoice(Invoice invoice, ref Response<string> response)
        {
            if (invoice is null)
            {
                response.IsSuccess = false;
                response.Data.Add("There is no invoice matching this payment");

                return;
            }

            if (invoice.Amount < 0)
            {
                response.IsSuccess = false;
                response.Data.Add("The amount is negative.");
            }

            if (invoice.AmountPaid < 0)
            {
                response.IsSuccess = false;
                response.Data.Add("The amount paid is negative.");
            }

            if (invoice.TaxAmount < 0)
            {
                response.IsSuccess = false;
                response.Data.Add("The tax amount is negative.");
            }

            if (invoice.Amount + invoice.TaxAmount == 0)
            {
                if (invoice.Payments?.Any() == true)
                {
                    response.IsSuccess = false;
                    response.Data.Add("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
                }
                else
                {
                    response.IsSuccess = false;
                    response.Data.Add("No payment needed.");
                }
            }

            if (invoice.Payments?.Sum(x => x.Amount) != invoice.AmountPaid)
            {
                response.IsSuccess = false;
                response.Data.Add("The sum of the payments is different from the amount paid.");
            }

            if (invoice.Amount + invoice.TaxAmount == invoice.AmountPaid)
            {
                response.IsSuccess = false;
                response.Data.Add("The invoice is already fully paid.");
            }

            if (invoice.Amount + invoice.TaxAmount < invoice.AmountPaid)
            {
                string tax = invoice.TaxAmount > 0 ? " + invoice.TaxAmount" : "";

                response.IsSuccess = false;
                response.Data.Add($"The amount paid is more than the amount{tax}.");
            }
        }
    }
}
