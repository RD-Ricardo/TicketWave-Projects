using Amazon.DynamoDBv2.DataModel;

namespace TicketWaveAccountApi.Services
{
    public class TenantService : ITenantService
    {
        private readonly IDynamoDBContext _dynamoDBContext;
        public TenantService(IDynamoDBContext dynamoDBContext)
        {
            _dynamoDBContext = dynamoDBContext;
        }

        public async Task CreateAsync(string name, string address, Guid userAdminId)
        {
            await _dynamoDBContext.SaveAsync(new Tenant
            {
                id = Guid.NewGuid().ToString(),
                Name = name,
                Address = address,
                UserAdminId = userAdminId.ToString()
            });
        }
    }

    [DynamoDBTable("tenant")]
    public class Tenant
    {
        [DynamoDBHashKey]
        public string id { get; set; } 

        [DynamoDBProperty("name")]
        public string Name { get; set; }

        [DynamoDBProperty("address")]
        public string Address { get; set; }

        [DynamoDBProperty("userAdminId")]

        public string UserAdminId { get; set; }
    }
}
