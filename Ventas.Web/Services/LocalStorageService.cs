namespace Ventas.Web.Services
{
    public class LocalStorageService : IStorageService
    {
        private readonly IWebHostEnvironment _env;

        public LocalStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "modelos");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string filePath = Path.Combine(uploadsFolder, fileName);
            using var fileOutput = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(fileOutput);

            return $"/uploads/modelos/{fileName}";
        }

        public Task DeleteFileAsync(string fileUrl)
        {
            if (!string.IsNullOrEmpty(fileUrl) && fileUrl.StartsWith("/uploads/"))
            {
                string oldPath = Path.Combine(_env.WebRootPath, fileUrl.TrimStart('/'));
                if (File.Exists(oldPath))
                {
                    File.Delete(oldPath);
                }
            }
            return Task.CompletedTask;
        }
    }
}
