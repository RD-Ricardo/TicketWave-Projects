using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using TicketWaveAccountApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITenantService, TenantService>();

builder.Services.AddSingleton<IAmazonDynamoDB>(_=> new AmazonDynamoDBClient("", "", Amazon.RegionEndpoint.USEast1));
builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

var app = builder.Build();

app.UseCors("AllowAllOrigins");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "Welcome to running ASP.NET Core Minimal API on AWS Lambda");

app.Run();

