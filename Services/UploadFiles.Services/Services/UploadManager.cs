using Microsoft.AspNetCore.Http;
using UploadFiles.Interfaces;
using UploadFiles.Services.Utils;
using FileTypeExt = (UploadFiles.Services.Utils.FileType, string);
namespace UploadFiles.Services
{
    public class UploadManager
    {
        private readonly IEnumerable<IFileHandler> _fileHandlers;

        public UploadManager(IEnumerable<IFileHandler> fileHandlers)
        {
            _fileHandlers = fileHandlers;
        }

        public async Task HandleUploadAsync(IFormFile file)
        {
            FileTypeExt fileType = DetermineFileType(file);

            var handler = _fileHandlers.FirstOrDefault(h => h.FileType == fileType.Item1);

            if (handler != null)
            {
                await handler.HandleFileAsync(file);
            }
            else
            {
            }
        }

        private FileTypeExt DetermineFileType(IFormFile file)
        {
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            switch (extension)
            {
                case ".doc":
                case ".docx":
                case ".pdf":
                    return (FileType.Document, "");

                case ".jpg":
                case ".jpeg":
                case ".png":
                    return (FileType.Document, "");
     
                default:
                    return (FileType.Document, ""); 
            }
        }
    }
}
