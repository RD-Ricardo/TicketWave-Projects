using Microsoft.Extensions.Configuration;

namespace ProcessPayment
{
    public interface IConfigurationService
    {
        IConfiguration GetConfiguration();
    }
}
