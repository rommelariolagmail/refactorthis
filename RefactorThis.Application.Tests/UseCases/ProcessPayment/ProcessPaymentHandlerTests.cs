using Moq;
using RefactorThis.Application.Interfaces;
using RefactorThis.Application.UseCases.ProcessPayment;
using RefactorThis.Domain.Entities;

namespace RefactorThis.Application.Tests.UseCases.ProcessPayment
{
    [TestFixture]
    public class ProcessPaymentHandlerTests
    {
        private Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private ProcessPaymentHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            _handler = new ProcessPaymentHandler(_invoiceRepositoryMock.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnErrorMessage_WhenReferenceIsMissing()
        {
            var command = new ProcessPaymentCommand { Reference = "", Amount = 100 };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Messages, Has.One.Contains("Missing payment reference."));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnErrorMessage_WhenAmountIsInvalid()
        {
            var command = new ProcessPaymentCommand { Reference = "INV123", Amount = -100 };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Messages, Has.One.Contains("Invalid payment amount."));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnErrorMessage_WhenInvoiceNotFound()
        {
            var command = new ProcessPaymentCommand { Reference = "INV123", Amount = 100 };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByReferenceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(value: null);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Messages, Has.One.Contains("There is no invoice matching this payment"));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnErrorMessage_WhenInvoiceHasNegativeAmounts()
        {
            var command = new ProcessPaymentCommand { Reference = "INV123", Amount = 100 };
            var invoice = new Invoice { Amount = -100, AmountPaid = 0, TaxAmount = 0, Payments = [] };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByReferenceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(invoice);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Messages, Has.One.Contains("The amount is negative."));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnErrorMessage_WhenInvoiceHasNegativeTaxAmounts()
        {
            var command = new ProcessPaymentCommand { Reference = "INV123", Amount = 100 };
            var invoice = new Invoice { Amount = 100, AmountPaid = 0, TaxAmount = -10, Payments = [] };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByReferenceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(invoice);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Messages, Has.One.Contains("The tax amount is negative."));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnErrorMessage_WhenInvoiceIsInInvalidState()
        {
            var command = new ProcessPaymentCommand { Reference = "INV123", Amount = 200 };
            var invoice = new Invoice
            {
                Amount = 0,
                AmountPaid = 0,
                TaxAmount = 0,
                Payments = [new Payment { Amount = 50 }]
            };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByReferenceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(invoice);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Messages, Has.One.Contains("The invoice is in an invalid state, it has an amount of 0 and it has payments."));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnErrorMessage_WhenInvoiceNeededNoPayment()
        {
            var command = new ProcessPaymentCommand { Reference = "INV123", Amount = 200 };
            var invoice = new Invoice
            {
                Amount = 0,
                AmountPaid = 0,
                TaxAmount = 0,
                Payments = []
            };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByReferenceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(invoice);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Messages, Has.One.Contains("No payment needed."));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnErrorMessage_WhenInvoicePaymentsNotAlignWithPaidAmount()
        {
            var command = new ProcessPaymentCommand { Reference = "INV123", Amount = 200 };
            var invoice = new Invoice
            {
                Amount = 100,
                AmountPaid = 80,
                TaxAmount = 0,
                Payments = [new Payment { Amount = 50 }, new Payment { Amount = 20 }]
            };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByReferenceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(invoice);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Messages, Has.One.Contains("The sum of the payments is different from the amount paid."));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnErrorMessage_WhenInvoiceIsAlreadyFullyPaid()
        {
            var command = new ProcessPaymentCommand { Reference = "INV123", Amount = 200 };
            var invoice = new Invoice
            {
                Amount = 200,
                AmountPaid = 200,
                TaxAmount = 0,
                Payments =
            [
                new() { Amount = 50 },
                new() { Amount = 150 }
            ]
            };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByReferenceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(invoice);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Messages, Has.One.Contains("The invoice is already fully paid."));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnErrorMessage_WhenOverPaymentOccurs()
        {
            var command = new ProcessPaymentCommand { Reference = "INV123", Amount = 200 };
            var invoice = new Invoice
            {
                Amount = 100,
                AmountPaid = 50,
                TaxAmount = 0,
                Payments =
            [
                new() { Amount = 50 }
            ]
            };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByReferenceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(invoice);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Messages, Has.One.Contains("Over payment."));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnPartialPaymentMessage_WithTax_WhenPartialPaymentIsMade()
        {
            var command = new ProcessPaymentCommand { Reference = "INV123", Amount = 50 };
            var invoice = new Invoice
            {
                Amount = 200,
                AmountPaid = 100,
                TaxAmount = 20,
                Payments =
            [
                new() { Amount = 50 },
                new() { Amount = 50 }
            ]
            };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByReferenceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(invoice);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Messages, Has.One.Contains("Invoice is now partially paid."));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnPartialPaymentMessage_WithoutTax_WhenPartialPaymentIsMade()
        {
            var command = new ProcessPaymentCommand { Reference = "INV123", Amount = 50 };
            var invoice = new Invoice
            {
                Amount = 200,
                AmountPaid = 100,
                TaxAmount = 0,
                Payments =
            [
                new() { Amount = 50 },
                new() { Amount = 50 }
            ]
            };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByReferenceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(invoice);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Messages, Has.One.Contains("Invoice is now partially paid."));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnFullPaymentMessage_WithTax_WhenFullPaymentIsMade()
        {
            var command = new ProcessPaymentCommand { Reference = "INV123", Amount = 100 };
            var invoice = new Invoice
            {
                Amount = 200,
                AmountPaid = 120,
                TaxAmount = 20,
                Payments =
            [
                new() { Amount = 120 }
            ]
            };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByReferenceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(invoice);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Messages, Has.One.Contains("Invoice is now fully paid."));
            });
        }


        [Test]
        public async Task Handle_ShouldReturnFullPaymentMessage_WithoutTax_WhenFullPaymentIsMade()
        {
            var command = new ProcessPaymentCommand { Reference = "INV123", Amount = 100 };
            var invoice = new Invoice
            {
                Amount = 200,
                AmountPaid = 100,
                TaxAmount = 0,
                Payments =
            [
                new() { Amount = 100 }
            ]
            };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByReferenceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(invoice);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Messages, Has.One.Contains("Invoice is now fully paid."));
            });
        }
    }
}