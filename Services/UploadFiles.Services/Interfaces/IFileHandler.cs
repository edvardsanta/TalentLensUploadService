using Microsoft.AspNetCore.Http;
using UploadFiles.Services.Utils;


namespace UploadFiles.Interfaces
{
    public interface IFileHandler
    {
        FileType FileType { get; }
        Task HandleFileAsync(IFormFile file);
    }
}
