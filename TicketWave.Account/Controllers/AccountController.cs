using Microsoft.AspNetCore.Mvc;
using TicketWaveAccountApi.Dtos;
using TicketWaveAccountApi.Services;

namespace TicketWaveAccountApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : Controller
    {

        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

      
        [HttpPost("login")]
        public async Task<IActionResult> Index(LoginDto dto)
        {
            return Ok(await _accountService.LoginAsync(dto));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            return Ok(await _accountService.RegisterUserAsync(dto));
        }

        [HttpPost("verify-code-email")]
        public async Task<IActionResult> VeriyCodeEmail(VerifyEmailDto dto)
        {
            await _accountService.VerifyCodeAsync(dto);
            return Ok();
        }
    }
}
