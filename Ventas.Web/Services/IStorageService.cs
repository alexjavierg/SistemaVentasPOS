namespace Ventas.Web.Services
{
    public interface IStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName);
        Task DeleteFileAsync(string fileUrl);
    }
}
