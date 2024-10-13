using Moq;
using RefactorThis.Application.Interfaces;
using RefactorThis.Application.UseCases.InvoiceCRUD.Handlers;
using RefactorThis.Application.UseCases.InvoiceCRUD.Queries;
using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Enums;

namespace RefactorThis.Application.Tests.UseCases.InvoiceCRUD
{
    [TestFixture]
    public class GetInvoiceByIdHandlerTests
    {
        private Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private GetInvoiceByIdHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            _handler = new GetInvoiceByIdHandler(_invoiceRepositoryMock.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnErrorResponse_WhenInvoiceNotFound()
        {
            // Arrange
            var query = new GetInvoiceByIdQuery { InvoiceId = Guid.NewGuid() };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByIdAsync(query.InvoiceId, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(value: null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.IsSuccess);
                Assert.That(result.Messages, Has.One.EqualTo("Invoice not found."));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnSuccessResponse_WhenInvoiceIsFound()
        {
            // Arrange
            var query = new GetInvoiceByIdQuery { InvoiceId = Guid.NewGuid() };
            var invoice = new Invoice { Id = query.InvoiceId, Amount = 100, Type = InvoiceType.Commercial };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByIdAsync(query.InvoiceId, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(invoice);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsTrue(result.IsSuccess);
                Assert.That(result.Data, Has.Exactly(1).InstanceOf<Invoice>());
                Assert.That(result.Data[0], Is.EqualTo(invoice));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnErrorResponse_WhenExceptionIsThrown()
        {
            // Arrange
            var query = new GetInvoiceByIdQuery { InvoiceId = Guid.NewGuid() };
            var exceptionMessage = "Database error";
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByIdAsync(query.InvoiceId, It.IsAny<CancellationToken>()))
                                  .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.IsSuccess);
                Assert.That(result.Messages, Has.One.EqualTo(exceptionMessage));
            });
        }
    }
}
