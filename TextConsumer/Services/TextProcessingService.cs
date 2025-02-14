using Microsoft.Extensions.Logging;
using TextConsumer.Services.Interfaces;
using TextProcessing;
using UploadFiles.Infrastructure.Persistence.Repositories;
using UploadFiles.Shared.Enums;

namespace TextConsumer.Services
{
    public class TextProcessingService : ITextProcessingService
    {
        private readonly IFileRepository _fileRepository;
        private readonly ILogger<TextProcessingService> _logger;
        ProcessingStep ProcessingStep = ProcessingStep.TextExtraction;
        public TextProcessingService(
            IFileRepository fileRepository,
            ILogger<TextProcessingService> logger)
        {
            _fileRepository = fileRepository;
            _logger = logger;
        }

        public async Task<string> NormalizeTextAsync(string originalText, string fileId, string jobId)
        {
            try
            {
                await _fileRepository.UpdateJobStepAsync(
                    fileId,
                    jobId,
                    ProcessingStep,
                    false);

                var processor = new Processing();
                string normalizedText = await processor.Process(
                    originalText,
                    "pt",
                    true);

                await UpdateProcessingResults(fileId, originalText, normalizedText);

                await _fileRepository.UpdateJobStepAsync(
                      fileId,
                      jobId,
                      ProcessingStep,
                      true);

                return normalizedText;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error processing text for FileId: {FileId}, JobId: {JobId}",
                    fileId, jobId);

                await HandleProcessingError(fileId, jobId, ex);
                throw;
            }
        }

        private async Task UpdateProcessingResults(string fileId, string originalText, string normalizedText)
        {
            var metadata = new Dictionary<string, object>
            {
                ["processingCompletedAt"] = DateTime.UtcNow,
                ["textLength"] = normalizedText.Length,
                ["textProcessingStatus"] = "Completed"
            };

            await _fileRepository.UpdateProcessingResultsAsync(
                fileId,
                originalText,
                normalizedText,
                "pt",
                new Dictionary<string, double>(),
                "1.0"
            );

            await _fileRepository.UpdateFileMetadataAsync(fileId, metadata);
        }

        private async Task HandleProcessingError(string fileId, string jobId, System.Exception exception)
        {
            try
            {
                await _fileRepository.UpdateJobStepAsync(
                    fileId,
                    jobId,
                    ProcessingStep.TextExtraction,
                    false,
                    exception.Message);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to update job status for failed processing. JobId: {JobId}", jobId);
            }
        }
    }
}
