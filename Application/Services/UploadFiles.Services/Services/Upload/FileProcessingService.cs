using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using UploadFiles.Infrastructure.Persistence.Repositories;
using UploadFiles.Services.Services.Upload.Interfaces;
using UploadFiles.Services.Services.Upload.Models;
using UploadFiles.Shared.Contracts;
using UploadFiles.Shared.Enums;
using UploadFiles.Shared.Models.File;
using UploadFiles.Shared.Models.Job;

namespace UploadFiles.Services.Services.Upload
{
    public class FileProcessingService : IFileProcessingService
    {
        private readonly IFileRepository _fileRepository;
        private readonly IPublishEndpoint? _publishEndpoint;
        private readonly UploadManager _uploadManager;
        private readonly ILogger<FileProcessingService> _logger;
        ProcessingStep ProcessingStep = ProcessingStep.FileValidation;


        public FileProcessingService(
            IFileRepository fileRepository,
            UploadManager uploadManager,
            ILogger<FileProcessingService> logger,
            IPublishEndpoint? publishEndpoint = null)
        {
            _fileRepository = fileRepository;
            _uploadManager = uploadManager;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<FileUploadResult> ProcessFileUploadAsync(IFormFile file, string userId)
        {
            FileDocument? fileDoc = new FileDocument
            {
                UserId = userId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                FileMetadata = new Dictionary<string, object>
                {
                    ["uploadStartedAt"] = DateTime.UtcNow
                }
            };
            JobDocument? job = new JobDocument
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Status = JobStatus.Created
            };

            try
            {
                fileDoc = await _fileRepository.CreateAsync(fileDoc);

                await _fileRepository.AddJobAsync(fileDoc.FileId.ToString(), job);

                var uploadResult = await _uploadManager.HandleUploadAsync(file);

                if (!uploadResult.SuccessIdentified)
                {
                    await _fileRepository.UpdateJobStepAsync(fileDoc.FileId, job.Id, ProcessingStep, false, uploadResult.Error);
                    return uploadResult;
                }

                await UpdateFileAndJobStep(fileDoc, job.Id, uploadResult);

                await PublishMessageAsync(
                    uploadResult.extractedText,
                    fileDoc.FileId.ToString(),
                    job.Id.ToString()
                );

                uploadResult.FileId = fileDoc.FileId;
                uploadResult.JobId = job.Id;
                return uploadResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file upload for {FileName}", file.FileName);

                if (job != null)
                {
                    await HandleFailedUpload(job.Id, fileDoc.FileId, ex);
                }

                throw new Exception("Failed to process file upload", ex);
            }
        }

        private async Task UpdateFileAndJobStep(
            FileDocument file,
            string jobId,
            FileUploadResult uploadResult
            )
        {
            Dictionary<string, object> metadata = new Dictionary<string, object>
            {
                ["fileType"] = uploadResult.FileType,
                ["fileExtension"] = uploadResult.FileExtension,
                ["uploadCompletedAt"] = DateTime.UtcNow
            };

            await Task.WhenAll(
                _fileRepository.UpdateFileExtension(file.FileId, uploadResult.FileExtension, uploadResult.FileType),
                _fileRepository.UpdateFileMetadataAsync(file.FileId, metadata),
                _fileRepository.UpdateJobStepAsync(file.FileId, jobId, ProcessingStep, true)
            );
        }

        private async Task HandleFailedUpload(string jobId, string fileId, Exception exception)
        {
            try
            {
                await _fileRepository.UpdateJobStepAsync(fileId, jobId, ProcessingStep, false, exception.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update job status for failed upload. JobId: {JobId}", jobId);
            }
        }

        private async Task<bool> PublishMessageAsync(string message, string fileId, string jobId)
        {
            try
            {
                if (_publishEndpoint == null)
                {
                    _logger.LogWarning("PublishEndpoint is not configured - skipping message publication");
                    return false;
                }

                var normalize = new NormalizeTextRecord(message, fileId, jobId);
                await _publishEndpoint.Publish(normalize);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message to queue for FileId: {FileId}, JobId: {JobId}", fileId, jobId);
                throw new Exception("Failed to queue file for processing", ex);
            }
        }

        public Task<(bool isValid, string message)> ValidateFileUploadAsync(string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
