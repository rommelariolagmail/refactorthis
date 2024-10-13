using Moq;
using RefactorThis.Application.Interfaces;
using RefactorThis.Application.UseCases.InvoiceCRUD.Commands;
using RefactorThis.Application.UseCases.InvoiceCRUD.Handlers;
using RefactorThis.Domain.Entities;

namespace RefactorThis.Application.Tests.UseCases.InvoiceCRUD
{
    [TestFixture]
    public class CreateInvoiceHandlerTests
    {
        private Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private CreateInvoiceHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            _handler = new CreateInvoiceHandler(_invoiceRepositoryMock.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnErrorResponse_WhenAmountIsInvalid()
        {
            // Arrange
            var command = new CreateInvoiceCommand { Amount = -100, Type = Domain.Enums.InvoiceType.Commercial };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.IsSuccess);
                Assert.That(result.Messages, Has.One.EqualTo("Invalid amount"));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnSuccessResponse_WhenInvoiceIsCreated()
        {
            // Arrange
            var command = new CreateInvoiceCommand { Amount = 100, Type = Domain.Enums.InvoiceType.Commercial };
            Invoice invoice = new()
            {
                Amount = command.Amount,
                Type = command.Type,
                AmountPaid = 0,
                TaxAmount = command.Type == Domain.Enums.InvoiceType.Commercial
                    ? command.Amount * 0.1M : 0,
                Payments = []
            };
            _invoiceRepositoryMock.Setup(repo => repo.CreateInvoiceAsync(invoice))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsTrue(result.IsSuccess);
                Assert.That(result.Messages, Has.One.EqualTo("Invoice created."));
                Assert.That(result.Data, Has.Exactly(1).InstanceOf<Invoice>());
                Assert.That(result.Data[0].Amount, Is.EqualTo(100));
                Assert.That(result.Data[0].TaxAmount, Is.EqualTo(10));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnErrorResponse_WhenExceptionIsThrown()
        {
            // Arrange
            var command = new CreateInvoiceCommand { Amount = 100, Type = Domain.Enums.InvoiceType.Commercial };
            _invoiceRepositoryMock.Setup(repo => repo.CreateInvoiceAsync(It.IsAny<Invoice>()))
                                  .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.That(result.Messages, Has.One.EqualTo("Database error"));
        }
    }
}
