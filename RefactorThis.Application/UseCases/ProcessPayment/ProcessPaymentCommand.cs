using MediatR;
using RefactorThis.Domain.Entities;

namespace RefactorThis.Application.UseCases.ProcessPayment
{
    public sealed class ProcessPaymentCommand : IRequest<Response<string>>
    {
        public string Reference { get; set; }
        public decimal Amount { get; set; }
    }
}
