using Microsoft.AspNetCore.Http;
using UploadFiles.Services.Services.Upload.Models;

namespace UploadFiles.Services.Services.Upload.Interfaces
{
    public interface IFileProcessingService
    {
        Task<(bool isValid, string message)> ValidateFileUploadAsync(string fileName);
        Task<FileUploadResult> ProcessFileUploadAsync(IFormFile file, string userId);
    }
}
