using FicMart.Domain.Enums;

namespace FicMart.Domain.Entities
{
    public class Receipt
    {
        public long Id { get; set; }                  
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public long Amount { get; set; }
        public Currency Currency { get; set; } = Currency.Dollars;

        public PaymentStatus PaymentStatus { get; set; }        

        public DateTime? AuthorizedAt { get; set; }
        public DateTime? CapturedAt { get; set; }
        public DateTime? VoidedAt { get; set; }
        public DateTime? RefundedAt { get; set; }

        public string? AuthorizationId { get; set; }    
        public string? CaptureId { get; set; }
        public string? VoidId { get; set; }
        public string? RefundId { get; set; }

    }
}
