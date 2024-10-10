using RefactorThis.Domain.Enums;
using System.Collections.Generic;

namespace RefactorThis.Domain.Entities
{
    public class Invoice : BaseEntity
    {
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TaxAmount { get; set; }
        public List<Payment> Payments { get; set; }

        public InvoiceType Type { get; set; }
    }
}