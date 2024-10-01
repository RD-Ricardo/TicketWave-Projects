namespace TicketApi.Event
{
    public class TicketPaidEvent
    {
        public DateTime PaidAt { get; set; }
        public Guid TicketId { get; set; }
        public string Status { get; set; } = "Paid";
    }
}
