using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

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
            // Usar la ruta compartida /app/storage mapeada en Docker
            var storageRoot = Path.Combine(Directory.GetCurrentDirectory(), "storage");
            var uploadsFolder = Path.Combine(storageRoot, "modelos");
            
            if (!Directory.Exists(uploadsFolder)) 
                Directory.CreateDirectory(uploadsFolder);

            string filePath = Path.Combine(uploadsFolder, fileName);
            using var fileOutput = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(fileOutput);

            // Devolver la ruta relativa estática
            return "/storage/modelos/" + fileName;
        }

        public Task DeleteFileAsync(string fileUrl)
        {
            if (!string.IsNullOrEmpty(fileUrl) && fileUrl.StartsWith("/storage/"))
            {
                var storageRoot = Path.Combine(Directory.GetCurrentDirectory(), "storage");
                // Remover el "/storage/" inicial para armar la ruta
                string relativePath = fileUrl.Substring(9); 
                string oldPath = Path.Combine(storageRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
                
                if (File.Exists(oldPath))
                {
                    File.Delete(oldPath);
                }
            }
            return Task.CompletedTask;
        }
    }
}
