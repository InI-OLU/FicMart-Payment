using FicMart.Domain.Enums;

namespace FicMart.Domain.Entities
{
    public class Receipt
    {
        public string PaymentReference { get; set; }
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public long Amount { get; set; }
        public Currency Currency { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime Timestamps { get; set; }
        public string BankReferenceId { get; set; }
    }
}
