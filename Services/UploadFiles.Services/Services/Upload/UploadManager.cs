using MassTransit;
using Microsoft.AspNetCore.Http;
using System.Collections.Immutable;
using UploadFiles.Services.Interfaces;
using UploadFiles.Services.Services.Upload.Models;
using UploadFiles.Services.Utils;
using UploadFiles.Shared.Contracts;
using FileTypeExt = (UploadFiles.Services.Utils.FileType type, UploadFiles.Services.Utils.FileExtension ext);
namespace UploadFiles.Services.Services.Upload
{
    public class UploadManager
    {
        private readonly ImmutableList<IFileHandler> _fileHandlers;
        private readonly IPublishEndpoint _publishEndpoint;

        public UploadManager(ImmutableList<IFileHandler> fileHandlers, IPublishEndpoint? publishEndpoint = null)
        {
            _fileHandlers = fileHandlers;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<FileUploadResult> HandleUploadAsync(IFormFile file)
        {   
            try
            {
                FileTypeExt fileType = DetermineFileTypeExt(file);

                IFileHandler? handler = _fileHandlers.FirstOrDefault(h => h.FileType == fileType);

                if (handler != null)
                {
                    RankTextMessage rankMessageToSend = await handler.HandleFileAsync(file);

                    if (_publishEndpoint != null)
                        await _publishEndpoint.Publish(rankMessageToSend);

                }
                else
                {
                }

                return new FileUploadResult(
                true,
                false,
                file.FileName,
                fileType.type.ToString(),
                fileType.ext.ToString(),
                DateTime.UtcNow
                );
            }
            catch(Exception ex)
            {
                return null;
            }     
        }

        private FileTypeExt DetermineFileTypeExt(IFormFile file)
        {
            return (DetermineFileType(file), DetermineFileExtension(file));
        }


        private FileExtension DetermineFileExtension(IFormFile file)
        {
            string extension = Path.GetExtension(file.FileName).ToUpperInvariant().TrimStart('.');
            if (!Enum.TryParse(extension, true, out FileExtension result))
            {
                return FileExtension.Unknown;
            }
            return result;
        }

        private FileType DetermineFileType(IFormFile file)
        {
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            switch (extension)
            {
                case ".doc":
                case ".docx":
                case ".pdf":
                    return FileType.Document;

                case ".jpg":
                case ".jpeg":
                case ".png":
                    return FileType.Image;

                default:
                    return FileType.Unknown;
            }
        }
    }
}
