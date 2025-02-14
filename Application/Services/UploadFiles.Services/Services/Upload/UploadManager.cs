using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using UploadFiles.Services.Interfaces;
using UploadFiles.Services.Mapper;
using UploadFiles.Services.Services.Upload.Models;
using FileTypeExt = (UploadFiles.Shared.Enums.FileType type, UploadFiles.Shared.Enums.FileExtension ext);
namespace UploadFiles.Services.Services.Upload
{
    public class UploadManager
    {
        private readonly ILogger<UploadManager> _logger;
        private readonly FileTypeMapper _fileTypeMapper;
        private readonly ImmutableDictionary<FileTypeExt, IFileHandler> _fileHandlers;

        public UploadManager(ImmutableList<IFileHandler> fileHandlers, ILogger<UploadManager> logger)
        {
            _fileHandlers = fileHandlers.ToImmutableDictionary(
                h => h.FileType,
                h => h);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileTypeMapper = new FileTypeMapper();
        }

        public async Task<FileUploadResult> HandleUploadAsync(IFormFile file)
        {
            ArgumentNullException.ThrowIfNull(file);

            try
            {
                var fileType = _fileTypeMapper.DetermineFileType(file);
                return await ProcessFileUploadAsync(file, fileType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file upload for {FileName}", file.FileName);
                return CreateFileUploadResult(false, file);
            }
        }

        private async Task<FileUploadResult> ProcessFileUploadAsync(IFormFile file, FileTypeExt fileType)
        {
            if (!_fileHandlers.TryGetValue(fileType, out var handler))
            {
                _logger.LogWarning("No handler found for file type {FileType}", fileType);
                var fileUpload =  CreateFileUploadResult(false, file, fileType);
                fileUpload.Error = $"We can´t process file type {fileType.ext}";
                return fileUpload;
            }

            var message = await handler.HandleFileAsync(file);

            return CreateFileUploadResult(true, file, fileType, message);
        }

        private static FileUploadResult CreateFileUploadResult(
            bool successExtracted,
            IFormFile file,
            FileTypeExt fileType = default,
            string extractedText = "")
        {
            return new FileUploadResult(
                successExtracted,
                file.FileName,
                extractedText,
                fileType.type.ToString() ?? string.Empty,
                fileType.ext.ToString() ?? string.Empty,
                DateTime.UtcNow
            );
        }
    }
}
