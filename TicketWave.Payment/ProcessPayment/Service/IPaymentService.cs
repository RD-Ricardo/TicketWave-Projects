using ProcessPayment.Events;
using ProcessPayment.Models;

namespace ProcessPayment.Service
{
    public interface IPaymentService
    {
        Task<Payment> GetPaymentByIdAsync(Guid id);
        Task CreateAsync(InitPaymentEvent @event);
        Task UpdateStatusAsync(ResponsePaymentEvent paymentEvent);
    }
}
