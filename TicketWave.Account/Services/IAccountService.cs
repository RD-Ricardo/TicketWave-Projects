using TicketWaveAccountApi.Dtos;

namespace TicketWaveAccountApi.Services
{
    public interface IAccountService
    {
        Task<string> RegisterUserAsync(RegisterDto dto);
        Task<object> LoginAsync(LoginDto dto);
        Task VerifyCodeAsync(VerifyEmailDto dto);
    }
}
