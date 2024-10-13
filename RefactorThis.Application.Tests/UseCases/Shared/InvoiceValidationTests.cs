using RefactorThis.Application.UseCases.Shared;
using RefactorThis.Domain.Entities;

namespace RefactorThis.Application.Tests.UseCases.Shared
{
    [TestFixture]
    public class InvoiceValidationTests
    {
        private Invoice _invoice;
        private Response<string> _response;

        [SetUp]
        public void SetUp()
        {
            _invoice = new Invoice();
            _response = new Response<string> { Messages = new List<string>() };
        }

        [Test]
        public void ValidateInvoice_ShouldReturnErrorMessage_WhenAmountIsNegative()
        {
            _invoice.Amount = -1;

            InvoiceValidation.ValidateInvoice(_invoice, ref _response);

            Assert.Multiple(() =>
            {
                Assert.That(_response.Messages, Has.One.Contains("The amount is negative."));
            });
        }

        [Test]
        public void ValidateInvoice_ShouldReturnErrorMessage_WhenAmountPaidIsNegative()
        {
            _invoice.AmountPaid = -1;

            InvoiceValidation.ValidateInvoice(_invoice, ref _response);

            Assert.Multiple(() =>
            {
                Assert.That(_response.Messages, Has.One.Contains("The amount paid is negative."));
            });
        }

        [Test]
        public void ValidateInvoice_ShouldReturnErrorMessage_WhenTaxAmountIsNegative()
        {
            _invoice.TaxAmount = -1;

            InvoiceValidation.ValidateInvoice(_invoice, ref _response);

            Assert.Multiple(() =>
            {
                Assert.That(_response.Messages, Has.One.Contains("The tax amount is negative."));
            });
        }

        [Test]
        public void ValidateInvoice_ShouldReturnErrorMessage_WhenInvoiceIsInInvalidState()
        {
            _invoice.Amount = 0;
            _invoice.TaxAmount = 0;
            _invoice.Payments = new List<Payment> { new Payment { Amount = 50 } };

            InvoiceValidation.ValidateInvoice(_invoice, ref _response);

            Assert.Multiple(() =>
            {
                Assert.That(_response.Messages, Has.One.Contains("The invoice is in an invalid state, it has an amount of 0 and it has payments."));
            });
        }

        [Test]
        public void ValidateInvoice_ShouldReturnErrorMessage_WhenPaymentsSumDoesNotMatchAmountPaid()
        {
            _invoice.AmountPaid = 100;
            _invoice.Payments = new List<Payment> { new Payment { Amount = 50 }, new Payment { Amount = 30 } };

            InvoiceValidation.ValidateInvoice(_invoice, ref _response);

            Assert.Multiple(() =>
            {
                Assert.That(_response.Messages, Has.One.Contains("The sum of the payments is different from the amount paid."));
            });
        }

        [Test]
        public void ValidateInvoice_ShouldReturnErrorMessage_WhenAmountPaidExceedsAmountPlusTax()
        {
            _invoice.Amount = 100;
            _invoice.TaxAmount = 20;
            _invoice.AmountPaid = 150;

            InvoiceValidation.ValidateInvoice(_invoice, ref _response);

            Assert.Multiple(() =>
            {
                Assert.That(_response.Messages, Has.One.Contains("The amount paid is more than the amount + invoice.TaxAmount."));
            });
        }
    }
}
