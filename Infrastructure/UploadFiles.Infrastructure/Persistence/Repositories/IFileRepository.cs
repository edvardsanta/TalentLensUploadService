using UploadFiles.Shared.Enums;
using UploadFiles.Shared.Models.File;
using UploadFiles.Shared.Models.Job;

namespace UploadFiles.Infrastructure.Persistence.Repositories
{
    public interface IFileRepository
    {
        Task<FileDocument> CreateAsync(FileDocument file);
        Task<bool> UpdateFileAsync(string fileId, FileDocument file);
        Task<bool> UpdateFileMetadataAsync(string fileId, Dictionary<string, object> metadata);
        Task<bool> UpdateFileExtension(string fileId, string fileExtension, string? fileType = null);
        Task<bool> UpdateProcessingResultsAsync(
            string fileId,
            string originalText,
            string normalizedText,
            string language,
            Dictionary<string, double> skillScores,
            string modelVersion);
        Task<bool> AddJobAsync(string fileId, JobDocument job);
        Task<bool> UpdateJobStatusAsync(string fileId, string jobId, JobStatus status);
        Task<bool> UpdateJobStepAsync(string fileId, string jobId, ProcessingStep step, bool isCompleted, string? errorMessage = null);
    }
}
