using Microsoft.AspNetCore.Http;
using UploadFiles.Services.Utils;
using FileTypeExt = (UploadFiles.Services.Utils.FileType, UploadFiles.Services.Utils.FileExtension);

namespace UploadFiles.Interfaces
{
    public interface IFileHandler
    {
        FileTypeExt FileType { get; }
        Task HandleFileAsync(IFormFile file);
    }
}
