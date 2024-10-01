using Microsoft.AspNetCore.Mvc;
using TicketWaveAccountApi.Services;

namespace TicketWaveAccountApi.Controllers
{

    [Route("tenant")]
    [ApiController]
    public class TenantController : Controller
    {
        private readonly ITenantService _tenantService;

        private readonly IConfiguration _configuration;

        public TenantController(ITenantService tenantService, IConfiguration configuration)
        {
            _tenantService = tenantService;
            _configuration = configuration;
        }


        [HttpGet("test")]
        public IActionResult Test()
        {
            var test = _configuration["teste"];
            return Ok(test);
        }
    }
}
