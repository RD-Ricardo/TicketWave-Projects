namespace ProcessPayment.Events
{
    public class InitPaymentEvent
    {
        public List<Guid> TicketIds { get; set; } = [];
        public Guid UserId { get; set; }
        public Guid EventId { get; set; }
        public int PriceCentsForTicket { get; set; }
        public string Description { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public string EmailUser { get; set; } = default!;
    }
}
