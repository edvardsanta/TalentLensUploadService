using Microsoft.AspNetCore.Http;
using UploadFiles.Interfaces;
using UploadFiles.Services.Utils;
using FileTypeExt = (UploadFiles.Services.Utils.FileType type, UploadFiles.Services.Utils.FileExtension ext);
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
            FileTypeExt fileType = DetermineFileTypeExt(file);

            IFileHandler? handler = _fileHandlers.FirstOrDefault(h => h.FileType == fileType);

            if (handler != null)
            {
                await handler.HandleFileAsync(file);
            }
            else
            {
            }
        }

        private FileTypeExt DetermineFileTypeExt(IFormFile file)
        {
            return (DetermineFileType(file), DetermineFileExtension(file));
        }


        private FileExtension DetermineFileExtension(IFormFile file) 
        {
            string extension = Path.GetExtension(file.FileName).ToUpperInvariant();
            if (!Enum.TryParse<FileExtension>(extension, true, out FileExtension result))
            {
                return FileExtension.Unknown;
            }
            return result;
        }

        private FileType DetermineFileType(IFormFile file)
        {
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            switch (extension)
            {
                case ".doc":
                case ".docx":
                case ".pdf":
                    return FileType.Document;

                case ".jpg":
                case ".jpeg":
                case ".png":
                    return FileType.Image;
     
                default:
                    return FileType.Unknown; 
            }
        }
    }
}
