using Amazon.DynamoDBv2.DataModel;

namespace TicketApi.Dtos
{
    public class TicketDto
    {
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
    }
}
