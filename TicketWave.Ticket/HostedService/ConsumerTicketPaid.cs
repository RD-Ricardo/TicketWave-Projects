using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TicketApi.Event;
using TicketApi.InputModels;
using TicketApi.Services;

namespace TicketApi.HostedService
{
    public class ConsumerTicketPaid : BackgroundService
    {
        private readonly IConnection _connection;

        private readonly IModel _channel;

        const string exchangeName = "ticket-wave";

        private readonly IServiceProvider _services;

        public ConsumerTicketPaid(IConfiguration configuration, IServiceProvider services)
        {
            _services = services;

            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"],
                Password = configuration["RabbitMQ:Password"],
                UserName = configuration["RabbitMQ:Username"],
                Port = 5671,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(30),
                Ssl = {
                    Enabled = true,
                    ServerName = configuration["RabbitMQ:Host"],
                },

            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();

                await ProcessData(Encoding.UTF8.GetString(body));

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume("ticket-paid", false, consumer);

            return Task.CompletedTask;
        }

        private async Task ProcessData(string message)
        {
            using var scope = _services.CreateScope();

            var ticketSerivce = scope.ServiceProvider.GetRequiredService<ITicketService>();

            var paymentEvent = JsonSerializer.Deserialize<TicketPaidEvent>(message);

            if (paymentEvent == null)
                return;

            await ticketSerivce!.UpdatePaidAsync(paymentEvent);
        }
    }
}
