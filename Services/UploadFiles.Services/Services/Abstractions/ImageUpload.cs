using Microsoft.AspNetCore.Http;
using UploadFiles.Services.Interfaces;
using FileTypeExt = (UploadFiles.Services.Utils.FileType, UploadFiles.Services.Utils.FileExtension);

namespace UploadFiles.Services.Services.Abstractions
{
    public abstract class ImageUpload : IFileHandler
    {
        public abstract FileTypeExt FileType { get; set; }

        public abstract Task HandleFileAsync(IFormFile file);
    }
}
