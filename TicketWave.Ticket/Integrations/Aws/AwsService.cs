
namespace TicketApi.Integrations.Aws
{
    public class AwsService : IAwsService
    {
        public string GetFileUrl(string fileName)
        {
            throw new NotImplementedException();
        }

        public string GetPreSignedUrl(string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<string> UploadFileAsync(IFormFile file, string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
