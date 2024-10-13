using RefactorThis.Domain.Entities;
using System.Linq;

namespace RefactorThis.Application.UseCases.Shared
{
    public static class InvoiceValidation
    {
        public static void ValidateInvoice(Invoice invoice, ref Response<string> response)
        {
            if (invoice.Amount < 0)
            {
                response.Messages.Add("The amount is negative.");
            }

            if (invoice.AmountPaid < 0)
            {
                response.Messages.Add("The amount paid is negative.");
            }

            if (invoice.TaxAmount < 0)
            {
                response.Messages.Add("The tax amount is negative.");
            }

            if (invoice.Amount + invoice.TaxAmount == 0 && invoice.Payments?.Any() == true)
            {
                response.Messages.Add("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
            }

            if (invoice.Payments?.Sum(x => x.Amount) != invoice.AmountPaid)
            {
                response.Messages.Add("The sum of the payments is different from the amount paid.");
            }

            if (invoice.Amount + invoice.TaxAmount < invoice.AmountPaid)
            {
                string tax = invoice.TaxAmount > 0 ? " + invoice.TaxAmount" : "";

                response.Messages.Add($"The amount paid is more than the amount{tax}.");
            }
        }
    }
}