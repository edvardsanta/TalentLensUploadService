using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using UploadFiles.Services.Interfaces;
using UploadFiles.Services.Mapper;
using UploadFiles.Services.Services.Upload.Models;
using UploadFiles.Shared.Contracts;
using FileTypeExt = (UploadFiles.Services.Utils.FileType type, UploadFiles.Services.Utils.FileExtension ext);
namespace UploadFiles.Services.Services.Upload
{
    public class UploadManager
    {
        private readonly IPublishEndpoint? _publishEndpoint;
        private readonly ILogger<UploadManager> _logger;
        private readonly FileTypeMapper _fileTypeMapper;
        private readonly ImmutableDictionary<FileTypeExt, IFileHandler> _fileHandlers;

        public UploadManager(ImmutableList<IFileHandler> fileHandlers, ILogger<UploadManager> logger, IPublishEndpoint? publishEndpoint = null)
        {
            _fileHandlers = fileHandlers.ToImmutableDictionary(
                h => h.FileType,
                h => h);
            _publishEndpoint = publishEndpoint;
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
                return CreateFileUploadResult(false, false, file);
            }
        }

        private async Task<FileUploadResult> ProcessFileUploadAsync(IFormFile file, FileTypeExt fileType)
        {
            if (!_fileHandlers.TryGetValue(fileType, out var handler))
            {
                _logger.LogWarning("No handler found for file type {FileType}", fileType);
                return CreateFileUploadResult(false, false, file, fileType);
            }

            var message = await handler.HandleFileAsync(file);
            await PublishMessageAsync(message);

            return CreateFileUploadResult(true, true, file, fileType);
        }

        private async Task PublishMessageAsync(NormalizeTextMessage message)
        {
            try
            {
                await _publishEndpoint.Publish(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message to queue");
                throw new Exception("Failed to queue file for processing", ex);
            }
        }

        private static FileUploadResult CreateFileUploadResult(
            bool successExtracted,
            bool fileSentToProcess,
            IFormFile file,
            FileTypeExt fileType = default)
        {
            return new FileUploadResult(
                successExtracted,
                fileSentToProcess,
                file.FileName,
                fileType.type.ToString() ?? string.Empty,
                fileType.ext.ToString() ?? string.Empty,
                DateTime.UtcNow
            );
        }
    }
}
