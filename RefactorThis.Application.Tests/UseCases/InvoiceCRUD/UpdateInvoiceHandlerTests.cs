using Moq;
using RefactorThis.Application.Interfaces;
using RefactorThis.Application.UseCases.InvoiceCRUD.Commands;
using RefactorThis.Application.UseCases.InvoiceCRUD.Handlers;
using RefactorThis.Domain.Entities;
using RefactorThis.Domain.Enums;

namespace RefactorThis.Application.Tests.UseCases.InvoiceCRUD
{
    [TestFixture]
    public class UpdateInvoiceHandlerTests
    {
        private Mock<IInvoiceRepository> _invoiceRepositoryMock;
        private UpdateInvoiceHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _invoiceRepositoryMock = new Mock<IInvoiceRepository>();
            _handler = new UpdateInvoiceHandler(_invoiceRepositoryMock.Object);
        }

        [Test]
        public async Task Handle_ShouldReturnErrorResponse_WhenInvoiceNotFound()
        {
            var command = new UpdateInvoiceCommand { InvoiceId = Guid.NewGuid() };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(value: null);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.IsSuccess);
                Assert.That(result.Messages, Has.One.EqualTo("Invoice not found."));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnErrorResponse_WhenValidationFails()
        {
            var command = new UpdateInvoiceCommand { InvoiceId = Guid.NewGuid(), Amount = -1 };
            var existingInvoice = new Invoice { Amount = 100, Type = InvoiceType.Commercial };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(existingInvoice);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.IsSuccess);
                Assert.That(result.Messages, Is.Not.Empty);
            });
        }

        [Test]
        public async Task Handle_ShouldReturnSuccessResponse_WhenInvoiceIsUpdated()
        {
            var command = new UpdateInvoiceCommand 
            { 
                InvoiceId = Guid.NewGuid(), 
                Amount = 200, 
                Type = InvoiceType.Commercial
            };
            var existingInvoice = new Invoice 
            { 
                Amount = 100, 
                Type = InvoiceType.Commercial,
                Payments = [],
                AmountPaid = 0,
                TaxAmount = 10
            };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(existingInvoice);
            _invoiceRepositoryMock.Setup(repo => repo.UpdateInvoiceAsync(It.IsAny<Invoice>()))
                                  .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(result.IsSuccess);
                Assert.That(result.Messages, Has.One.EqualTo("Invoice updated."));
            });
        }

        [Test]
        public async Task Handle_ShouldReturnErrorResponse_WhenExceptionIsThrown()
        {
            var command = new UpdateInvoiceCommand { InvoiceId = Guid.NewGuid(), Amount = 200, Type = InvoiceType.Commercial };
            _invoiceRepositoryMock.Setup(repo => repo.GetInvoiceByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                  .Throws(new Exception("Database error"));

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.IsSuccess);
                Assert.That(result.Messages, Has.One.EqualTo("Database error"));
            });
        }
    }
}
