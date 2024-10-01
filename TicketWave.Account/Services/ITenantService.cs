namespace TicketWaveAccountApi.Services
{
    public interface ITenantService
    {
        Task CreateAsync(string name, string address, Guid userAdminId);
    }
}
