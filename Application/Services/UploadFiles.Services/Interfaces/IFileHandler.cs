using Microsoft.AspNetCore.Http;
using FileTypeExt = (UploadFiles.Shared.Enums.FileType, UploadFiles.Shared.Enums.FileExtension);

namespace UploadFiles.Services.Interfaces
{
    public interface IFileHandler
    {
        FileTypeExt FileType { get; }
        Task<string> HandleFileAsync(IFormFile file);
    }
}
