using Microsoft.AspNetCore.Http;
using UploadFiles.Interfaces;
using UploadFiles.Services.Utils;
using FileTypeExt = (UploadFiles.Services.Utils.FileType, UploadFiles.Services.Utils.FileExtension);

namespace UploadFiles.Services.Services.Abstractions
{
    public abstract class DocumentUpload : IFileHandler
    {
        public abstract FileTypeExt FileType { get; set; }

        public abstract Task HandleFileAsync(IFormFile file);
    }
}
