using Microsoft.AspNetCore.Http;
using UploadFiles.Interfaces;
using UploadFiles.Services.Utils;
namespace UploadFiles.Services.Services.Abstractions
{
    public abstract class DocumentUpload : IFileHandler
    {
        public abstract FileType FileType { get; set; }

        public abstract Task HandleFileAsync(IFormFile file);
    }
}
