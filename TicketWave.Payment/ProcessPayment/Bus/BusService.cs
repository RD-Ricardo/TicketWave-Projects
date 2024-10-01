using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ProcessPayment.Bus
{
    public class BusService : IBusService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchangeName;
        private readonly IConfiguration _configuration;

        public BusService(IConfiguration configuration)
        {
            _configuration = configuration;

            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"],
                Password = _configuration["RabbitMQ:Password"],
                UserName = _configuration["RabbitMQ:Username"],
                Port = 5671,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
                Ssl = {
                    Enabled = true,
                    ServerName = _configuration["RabbitMQ:Host"],
                },
            };

            _connection = factory.CreateConnection();

            _channel = _connection.CreateModel();

            _exchangeName = "ticket-wave";

            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Direct, true, false);

        }

        public Task PublishAsync<T>(string routingKey, T message)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            _channel.BasicPublish(_exchangeName, routingKey, null, body);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
