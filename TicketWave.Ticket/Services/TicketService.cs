using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.S3.Transfer;
using Amazon.S3;
using QRCoder;
using QuestPDF.Fluent;
using TicketApi.Bus;
using TicketApi.Event;
using TicketApi.InputModels;
using TicketApi.Models;
using Amazon.S3.Model;

namespace TicketApi.Services
{
    public class TicketService : ITicketService
    {
        private readonly IDynamoDBContext _dynamoDBContext;

        private readonly IBusService _busService;

        private readonly IConfiguration _configuration;

        public TicketService(IDynamoDBContext dynamoDBContext, IBusService busService, IConfiguration configuration)
        {
            _dynamoDBContext = dynamoDBContext;
            _busService = busService;
            _configuration = configuration;
        }

        public async Task CreateAsync(InitProcessTicketEvent initProcessTicketEvent)
        {
            var ticketIds = new List<Guid>();

            for (int i = 0; i < initProcessTicketEvent.Quantity; i++)
            {
                var ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    Name = initProcessTicketEvent.EventName ?? "Ingresso evento",
                    UserId = initProcessTicketEvent.UserId,
                    EventId = initProcessTicketEvent.EventId,
                    Price = initProcessTicketEvent.PriceCents,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    Status = "Reserved",
                    Description = null,
                };

                await _dynamoDBContext.SaveAsync(ticket);

                ticketIds.Add(ticket.Id);
            }

            var initPamentEvent = new InitPaymentEvent
            {
                TicketIds = ticketIds,
                UserId = initProcessTicketEvent.UserId,
                EventId = initProcessTicketEvent.EventId,
                PriceCentsForTicket = initProcessTicketEvent.PriceCents,
                Description = initProcessTicketEvent.EventName ?? "Ingresso evento",
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                EmailUser = initProcessTicketEvent.EmailUser
            };
            
            await _busService.PublishAsync("init-payment", initPamentEvent);
        }

        public async Task<Ticket> GetByIdAsync(Guid id)
        {
            var response = await _dynamoDBContext.LoadAsync<Ticket>(id);
            return response;
        }

        public async Task<List<Ticket>> GetByUserIdAsync(Guid userId)
        {
            var tickets = await _dynamoDBContext.ScanAsync<Ticket>(new List<ScanCondition>
            {
               new ScanCondition("UserId", ScanOperator.Equal, userId)
            },
            new DynamoDBOperationConfig
            {
                IndexName = "UserId-index"
            }).GetRemainingAsync();


            foreach (var ticket in tickets)
            {
                if(ticket.Status == "paid" && !string.IsNullOrEmpty(ticket.UrlTicket))
                {
                    var presignedUrl = GeneratePresignedUrlAsync(ticket.UrlTicket);
                    ticket.UrlTicket = presignedUrl;
                }
            }

            return tickets;
        }

        public async Task UpdatePaidAsync(TicketPaidEvent @event)
        {
            var ticket = await GetByIdAsync(@event.TicketId);
            ticket.Status = @event.Status;
            ticket.PaidAt = DateTime.UtcNow;

            string qrText = ticket.Id.ToString();
            var qrCodeImage = GenerateQrCode(qrText);

            byte[] pdfBytes = CreatePdfWithQrCode(qrCodeImage, qrText);

            var key = $"{ticket.Id}.pdf";

            await UploadPdfToS3(pdfBytes, key);

            ticket.UrlTicket = key;
            
            await _dynamoDBContext.SaveAsync(ticket);
        }

        private byte[] GenerateQrCode(string text)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                return qrCodeImage;
            }
        }

        private byte[] CreatePdfWithQrCode(byte[] qrCodeImage, string ticketId)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            return QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text($"Ticket - {ticketId}").FontSize(20).Bold();
                    page.Content().Image(qrCodeImage);
                    page.Footer().AlignCenter().Text("Ticket wave").FontSize(12);
                });
            }).GeneratePdf();
        }

        private string GeneratePresignedUrlAsync(string key)
        {
            using (var s3Client = new AmazonS3Client(_configuration["Amazon:AccessKey"], _configuration["Amazon:Secret"], Amazon.RegionEndpoint.USEast1)) // Ajuste a região conforme necessário
            {
                var request = new GetPreSignedUrlRequest { Key = key, BucketName = "ticket-wave-ticket", Expires = DateTime.UtcNow.AddMinutes(5) };

                var url = s3Client.GetPreSignedURL(request);

                return url;
            }
        }

        private async Task UploadPdfToS3(byte[] pdfBytes, string fileName)
        {
            using (var s3Client = new AmazonS3Client(_configuration["Amazon:AccessKey"], _configuration["Amazon:Secret"], Amazon.RegionEndpoint.USEast1)) // Ajuste a região conforme necessário
            {
                using (var stream = new MemoryStream(pdfBytes))
                {
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        InputStream = stream,
                        Key = fileName,
                        BucketName = "ticket-wave-ticket",
                        ContentType = "application/pdf",
                    };

                    var transferUtility = new TransferUtility(s3Client);
                    await transferUtility.UploadAsync(uploadRequest);
                }
            }
        }


    }
}
