using Microsoft.AspNetCore.Http;
using UploadFiles.Services.Interfaces;
using FileTypeExt = (UploadFiles.Shared.Enums.FileType, UploadFiles.Shared.Enums.FileExtension);

namespace UploadFiles.Services.Services.Upload.Abstractions
{
    public abstract class ImageUpload : IFileHandler
    {
        public abstract FileTypeExt FileType { get; set; }

        public abstract Task<string> HandleFileAsync(IFormFile file);
    }
}
