using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using TicketApi.HostedService;
using TicketApi.Services;
using TicketApi.Bus;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IAmazonDynamoDB>(_ => new AmazonDynamoDBClient("", "", Amazon.RegionEndpoint.USEast1));
builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();
builder.Services.AddScoped<ITicketService, TicketService>();   
builder.Services.AddScoped<IBusService, BusService>();
builder.Services.AddHostedService<ConsumerTicketReserve>();
builder.Services.AddHostedService<ConsumerTicketPaid>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
