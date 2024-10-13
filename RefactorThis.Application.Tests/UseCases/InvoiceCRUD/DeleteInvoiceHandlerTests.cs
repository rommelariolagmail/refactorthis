using Moq;
using RefactorThis.Application.Interfaces;
using RefactorThis.Application.UseCases.InvoiceCRUD.Commands;
using RefactorThis.Application.UseCases.InvoiceCRUD.Handlers;
using RefactorThis.Domain.Entities;

namespace RefactorThis.Application.Tests.UseCases.InvoiceCRUD
{
    [TestFixture]
    public class DeleteInvoiceHandlerTests
    {
        private Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private DeleteInvoiceHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            _handler = new DeleteInvoiceHandler(_invoiceRepositoryMock.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnErrorResponse_WhenInvoiceNotFound()
        {
            // Arrange
            var command = new DeleteInvoiceCommand { InvoiceId = Guid.NewGuid() };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(value: null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.IsSuccess);
                Assert.That(result.Messages, Has.One.EqualTo("Invoice not found."));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnSuccessResponse_WhenInvoiceIsDeleted()
        {
            // Arrange
            var command = new DeleteInvoiceCommand { InvoiceId = Guid.NewGuid() };
            var invoice = new Invoice { Id = command.InvoiceId };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(invoice);
            _invoiceRepositoryMock.Setup(repo => repo.DeleteInvoiceAsync(It.IsAny<Guid>()))
                                  .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsTrue(result.IsSuccess);
                Assert.That(result.Data, Has.One.EqualTo("Invoice deleted."));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnErrorResponse_WhenExceptionIsThrown()
        {
            // Arrange
            var command = new DeleteInvoiceCommand { InvoiceId = Guid.NewGuid() };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.That(result.Messages, Has.One.EqualTo("Database error"));
        }
    }
}
