using Amazon.DynamoDBv2.DataModel;

namespace TicketApi.Models
{
    [DynamoDBTable("ticket")]
    public class Ticket
    {
        [DynamoDBHashKey("Id")]
        public Guid Id { get; set; }

        [DynamoDBProperty]
        [DynamoDBGlobalSecondaryIndexHashKey("UserId-index")]
        public Guid UserId { get; set; }

        [DynamoDBProperty]
        [DynamoDBGlobalSecondaryIndexHashKey("EventId-index")]
        public Guid EventId { get; set; }

        [DynamoDBProperty]
        public string? Name { get; set; } = "Ticket";

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; }

        [DynamoDBProperty]
        public DateTime ExpiresAt { get; set; }

        [DynamoDBProperty]
        public DateTime? PaidAt { get; set; }

        [DynamoDBProperty]
        public string? Description { get; set; }

        [DynamoDBProperty]
        public int Price { get; set; }

        [DynamoDBProperty]
        public string Status { get; set; } = "Reserved";

        [DynamoDBProperty]
        public string UrlTicket { get; set; } = string.Empty;
    } 
}
