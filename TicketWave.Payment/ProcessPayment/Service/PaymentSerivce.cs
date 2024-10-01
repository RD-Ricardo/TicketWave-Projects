using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.SimpleEmail.Model;
using Amazon.SimpleEmail;
using ProcessPayment.Asass;
using ProcessPayment.Bus;
using ProcessPayment.Events;
using ProcessPayment.Models;
using RestSharp;
using System.Text.Json;
using System.Runtime.InteropServices;

namespace ProcessPayment.Service
{
    public class PaymentSerivce : IPaymentService
    {
        private readonly IDynamoDBContext _dBContext;

        private readonly IBusService _busService;

        private readonly IAmazonSimpleEmailService _amazonSimpleEmailServiceClient;
        public PaymentSerivce(IDynamoDBContext dBContext, IBusService busService, IAmazonSimpleEmailService amazonSimpleEmailServiceClient)
        {
            _dBContext = dBContext;
            _busService = busService;
            _amazonSimpleEmailServiceClient = amazonSimpleEmailServiceClient;
        }

        public async Task<Payment> GetPaymentByIdAsync(Guid id)
        {
            return await _dBContext.LoadAsync<Payment>(id);
        }

        public async Task CreateAsync(InitPaymentEvent @event)
        {
            var priceCents = @event.PriceCentsForTicket * @event.TicketIds.Count;

            var createPaymentDto = new CreatePaymentDto
            {
                billingType = "CREDIT_CARD",
                chargeType = "DETACHED",
                name = @event.Description,
                endDate = @event.ExpiresAt.ToString("yyyy-MM-dd"),
                value =  priceCents / 100m
            };

            var paymentResponse = await NewPayment(createPaymentDto);

            if (paymentResponse == null)
            {
                throw new Exception("Error creating payment");
            }

            foreach (var item in @event.TicketIds)
            {
                var payment = new Payment
                {
                    Id = Guid.NewGuid(),
                    Description = @event.Description,
                    PriceCents = @event.PriceCentsForTicket,
                    ExpiresAt = @event.ExpiresAt,
                    Status = Status.Pending,
                    UserId = @event.UserId,
                    EventId = @event.EventId,
                    TicketId = item,
                    PaidAt = null,
                    ExtId = paymentResponse.id,
                    PaymentLink = paymentResponse.url
                };

                await _dBContext.SaveAsync(payment);
            }

            await SendEmailAsync(@event.EmailUser,
                GetEmailBodyHtml(paymentResponse.url),
                GetEmailBodyText(paymentResponse.url)
            );

        }

        private string GetEmailBodyHtml(string url)
        {
            return $"<html><body><p>Olá, clique no link abaixo para pagar seus ingressos:</p><a href='{url}'>{url}</a></body></html>";
        }

        private string GetEmailBodyText( string url)
        {
            return $"Olá, clique no link abaixo para pagar seus ingressos: {url}";
        }

        public async Task UpdateStatusAsync(ResponsePaymentEvent paymentEvent)
        {
            var payments = await _dBContext.ScanAsync<Payment>(new List<ScanCondition>
            {
               new("ExtId", ScanOperator.Equal, paymentEvent.ExtId)
            },
            new DynamoDBOperationConfig
            {
                IndexName = "ExtId-index"
            }).GetRemainingAsync();

            var payment = payments.FirstOrDefault();    

            if (payment == null)
            {
                throw new Exception($"Payment not found {payment?.ExtId}");
            }

            payment.Status = GetStatus(paymentEvent.Status);

            if (payment.Status == Status.Paid)
            {
                payment.PaidAt = DateTime.UtcNow;

                await _busService.PublishAsync("ticket-paid", new TicketPaidEvent
                {
                    TicketId = payment.TicketId,
                    UserId = payment.UserId,
                    Status = "paid"
                });
            }

            await _dBContext.SaveAsync(payment);
        }



        public async Task SendEmailAsync(string emailTo,
            string bodyHtml, 
            string bodyText)
        {
            try
            {
                var response = await _amazonSimpleEmailServiceClient.SendEmailAsync(
                    new SendEmailRequest
                    {
                        Destination = new Destination
                        {
                            ToAddresses = [emailTo]
                        },
                        Message = new Message
                        {
                            Body = new Body
                            {
                                Html = new Content
                                {
                                    Charset = "UTF-8",
                                    Data = bodyHtml
                                },
                                Text = new Content
                                {
                                    Charset = "UTF-8",
                                    Data = bodyText
                                }
                            },
                            Subject = new Content
                            {
                                Charset = "UTF-8",
                                Data = "Pagamento de ingressos"
                            }
                        },
                        Source = "rdsolutions@ticketwave.awsapps.com"
                    });

                var messageId = response.MessageId;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendEmailAsync failed with exception: " + ex.Message);
            }
        }


        private async Task<ResponsePaymentDto?> NewPayment(CreatePaymentDto createPaymentDto)
        {
            try
            {
                using (var client = new RestClient(new RestClientOptions("https://sandbox.asaas.com/api/v3")))
                {
                    var request = new RestRequest("/paymentLinks");

                    request.AddHeader("accept", "application/json");
                    request.AddHeader("access_token", "$==");

                    var json = JsonSerializer.Serialize(createPaymentDto);

                    request.AddJsonBody(json);

                    var response = await client.PostAsync(request);

                    if (!response.IsSuccessStatusCode)
                        return null;

                    return JsonSerializer.Deserialize<ResponsePaymentDto>(response.Content!);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private Status GetStatus(string status)
        {
            return status switch
            {
                "pending" => Status.Pending,
                "paid" => Status.Paid,
                "failed" => Status.Failed,
                _ => throw new Exception("Invalid status")
            };
        }
    }
}
