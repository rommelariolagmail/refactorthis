using MediatR;
using RefactorThis.Application.Interfaces;
using RefactorThis.Application.UseCases.InvoiceCRUD.Commands;
using RefactorThis.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorThis.Application.UseCases.InvoiceCRUD.Handlers
{
    public sealed class CreateInvoiceHandler : IRequestHandler<CreateInvoiceCommand, Response<Invoice>>
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public CreateInvoiceHandler(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<Response<Invoice>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
        {
            Response<Invoice> response = new Response<Invoice>();

            try
            {
                #region request validation

                if (request.Amount <= 0)
                {
                    response.IsSuccess = false;
                    response.Messages.Add("Invalid amount");
                }

                if (response.Messages.Any())
                {
                    response.IsSuccess = false;

                    return response;
                }
                #endregion

                Invoice invoice = new Invoice
                {
                    Id = new Guid(),
                    Amount = request.Amount,
                    Type = request.Type,
                    AmountPaid = 0,
                    TaxAmount = request.Type == Domain.Enums.InvoiceType.Commercial
                        ? request.Amount * 0.1M : 0,
                    Payments = new List<Payment>()
                };

                await _invoiceRepository.CreateInvoiceAsync(invoice);

                response.Messages.Add("Invoice created.");
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
