using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.SimpleEmail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ProcessPayment.Bus;
using ProcessPayment.Events;
using ProcessPayment.Service;
using System.Text;


[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ProcessPayment;

public class Function
{
    private readonly IConfigurationService _configService;

    private readonly IPaymentService _paymentSerivce;
    public Function()
    {
        var serviceCollection = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables()
              .Build();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddSingleton<IConfigurationService, ConfigurationService>();
        serviceCollection.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient(configuration["AWS:AccessKeyId"], configuration["AWS:Secret"], Amazon.RegionEndpoint.USEast1));
        serviceCollection.AddSingleton<IAmazonSimpleEmailService>(_ => new AmazonSimpleEmailServiceClient(configuration["AWS:AccessKeyId"], configuration["AWS:Secret"], Amazon.RegionEndpoint.USEast1));
        
        serviceCollection.AddScoped<IPaymentService, PaymentSerivce>();
        serviceCollection.AddScoped<IBusService, BusService>();
        serviceCollection.AddScoped<IDynamoDBContext, DynamoDBContext>();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        _configService = serviceProvider.GetRequiredService<IConfigurationService>();
        _paymentSerivce = serviceProvider.GetRequiredService<IPaymentService>()!;
    }

    public async Task FunctionHandler(object @event, ILambdaContext context)
    {
        var eventMessage = JsonConvert.DeserializeObject<RabbitMQEvent>(@event.ToString()!);

        if (eventMessage?.RmqMessagesByQueue == null || eventMessage.RmqMessagesByQueue.Count == 0)
        {
            LambdaLogger.Log("Invalid event data");
            return;
        }
                   

        foreach (var queue in eventMessage.RmqMessagesByQueue)
        {
            LambdaLogger.Log($"Queue: {queue.Key}");

            if(queue.Key == "init-payment::/") 
            {
                foreach (var message in queue.Value)
                {
                    LambdaLogger.Log($"Message: {message.Data}");

                    byte[] data = Convert.FromBase64String(message.Data);
                    string jsonString = Encoding.UTF8.GetString(data);

                    var payment = JsonConvert.DeserializeObject<InitPaymentEvent>(jsonString);

                    if (payment == null)
                    {
                        LambdaLogger.Log("Invalid payment data");
                        return;
                    }

                    await _paymentSerivce.CreateAsync(payment);
                    
                    LambdaLogger.Log("Payment processed successfully");
                }
            }

            if(queue.Key == "response-payment::/")
            {
                foreach (var message in queue.Value)
                {
                    LambdaLogger.Log($"Message: {message.Data}");

                    byte[] data = Convert.FromBase64String(message.Data);
                    string jsonString = Encoding.UTF8.GetString(data);

                    var payment = JsonConvert.DeserializeObject<ResponsePaymentEvent>(jsonString);

                    LambdaLogger.Log($"Payment: {jsonString}");

                    if (payment == null)
                    {
                        LambdaLogger.Log("Invalid payment data");
                        return;
                    }

                    await _paymentSerivce.UpdateStatusAsync(payment);

                    LambdaLogger.Log("Payment updated successfully");
                }
            }
        }
    }

    private void ConfigureServices(IServiceCollection serviceCollection)
    {
        
    }
}
