namespace TicketApi.Integrations.Aws
{
    public interface IAwsService
    {
        Task<string> UploadFileAsync(IFormFile file, string fileName);
        string GetFileUrl(string fileName);
        string GetPreSignedUrl(string fileName);
    }
}
