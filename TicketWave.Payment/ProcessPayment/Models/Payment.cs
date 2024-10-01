using Amazon.DynamoDBv2.DataModel;

namespace ProcessPayment.Models
{
    [DynamoDBTable("payment")]
    public class Payment
    {
        [DynamoDBHashKey("id")]
        public Guid Id { get; set; }

        [DynamoDBProperty]
        public string Description { get; set; }

        [DynamoDBProperty]
        public int PriceCents { get; set; }

        [DynamoDBProperty]
        public DateTime ExpiresAt { get; set; }

        [DynamoDBProperty]
        public Status Status { get; set; }

        [DynamoDBProperty]
        [DynamoDBGlobalSecondaryIndexHashKey("Ext-index")]
        public string ExtId { get; set; }

        [DynamoDBProperty]
        public Guid UserId { get; set; }
        
        [DynamoDBProperty]
        public Guid EventId { get; set; }

        [DynamoDBProperty]
        public DateTime? PaidAt { get; set; }

        [DynamoDBProperty]
        public Guid TicketId { get; set; }

        [DynamoDBProperty]
        public string PaymentLink { get; set; } = null!;
    }

    public enum Status
    {
        Pending,
        Paid,
        Failed
    }
}