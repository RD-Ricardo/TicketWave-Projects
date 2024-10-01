namespace TicketApi.InputModels
{
    public class InitProcessTicketEvent
    {
        public Guid UserId { get; set; }
        public Guid EventId { get; set; }
        public int Quantity { get; set; }
        public int PriceCents { get; set; }
        public string EmailUser { get; set; } = default!;
        public string Cpf { get; set; } = default!;
        public string? EventName { get; set; }
    }
}
