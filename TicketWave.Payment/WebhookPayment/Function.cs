using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using WebhookPayment.Bus;
using WebhookPayment.Events;
using WebhookPayment.Input;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace WebhookPayment;

public class Function
{
    private readonly IBusService _busService;

    public Function()
    {
        var serviceCollection = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables()
              .Build();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddScoped<IBusService, BusService>();
        
        var serviceProvider = serviceCollection.BuildServiceProvider();

        _busService = serviceProvider.GetRequiredService<IBusService>();
    }

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(APIGatewayHttpApiV2ProxyRequest input, ILambdaContext context)
    {
        LambdaLogger.Log("Received input: " + JsonSerializer.Serialize(input));

        var webhook = JsonSerializer.Deserialize<WebhookPaymentDto>(input.Body);

        if (webhook == null)
        {
            LambdaLogger.Log("Invalid event data");
            
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 400,
                Body = "Invalid event data",
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };
        }


        if (webhook.@event.Equals("PAYMENT_CONFIRMED"))
        {
            LambdaLogger.Log($"Payment confirmed: {webhook.payment.invoiceUrl}");

            var paymentEvent = new ResponsePayment
            {
                ExtId = webhook!.payment.paymentLink,
                Status = "paid",
            };

            await _busService.PublishAsync("response-payment", paymentEvent);
        }

        return new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200,
            Body = "Ok",
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        };
    }
}
