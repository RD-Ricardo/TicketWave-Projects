namespace ProcessPayment.Events
{
    public class ResponsePaymentEvent
    {
        public string ExtId { get; set; }
        public string Status { get; set; }
    }
}
