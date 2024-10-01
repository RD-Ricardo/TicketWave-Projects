using TicketApi.Event;
using TicketApi.InputModels;
using TicketApi.Models;

namespace TicketApi.Services
{
    public interface ITicketService
    {
        Task<Ticket> GetByIdAsync(Guid id);
        Task<List<Ticket>> GetByUserIdAsync(Guid userId);
        Task CreateAsync(InitProcessTicketEvent initProcessTicketEvent);
        Task UpdatePaidAsync(TicketPaidEvent @event);
    }
}
