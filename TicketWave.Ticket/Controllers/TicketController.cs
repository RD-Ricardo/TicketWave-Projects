using Microsoft.AspNetCore.Mvc;
using TicketApi.Services;

namespace TicketApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;
        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetAllbyUser()
        {
            HttpContext.Request.Headers.TryGetValue("userId", out var userId);
            var tickets = await _ticketService.GetByUserIdAsync(Guid.Parse(userId!));
            return Ok(tickets);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);
            return Ok(ticket);
        }
    }
}
