using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using UploadFiles.Shared.Enums;
using UploadFiles.Shared.Models.File;
using UploadFiles.Shared.Models.Job;

namespace UploadFiles.Infrastructure.Persistence.Repositories
{
    public class FileRepository : BaseFileRepository, IFileRepository
    {
        public FileRepository(
            IMongoDatabase database,
            ILogger<FileRepository> logger) : base(database, logger)
        {
        }

        public async Task<FileDocument> CreateAsync(FileDocument file)
        {
            try
            {
                await _files.InsertOneAsync(file);
                return file;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating file {FileId}", file.FileId);
                throw;
            }
        }


        public async Task<bool> UpdateFileAsync(string fileId, FileDocument file)
        {
            try
            {
                file.UpdatedAt = DateTime.UtcNow;
                var filter = Builders<FileDocument>.Filter.Eq(f => f.FileId, fileId);
                var result = await _files.ReplaceOneAsync(filter, file);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating file {FileId}", fileId);
                throw;
            }
        }

        public async Task<bool> UpdateFileMetadataAsync(string fileId, Dictionary<string, object> metadata)
        {
            try
            {
                var filter = Builders<FileDocument>.Filter.Eq(f => f.FileId, fileId);
                var update = Builders<FileDocument>.Update
                    .Set(f => f.FileMetadata, metadata)
                    .Set(f => f.UpdatedAt, DateTime.UtcNow);

                var result = await _files.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating metadata for file {FileId}", fileId);
                throw;
            }
        }

        public async Task<bool> UpdateProcessingResultsAsync(
            string fileId,
            string? originalText,
            string normalizedText,
            string language,
            Dictionary<string, double> skillScores,
            string modelVersion)
        {
            try
            {
                var filter = Builders<FileDocument>.Filter.Eq(f => f.FileId, fileId);

                var update = Builders<FileDocument>.Update
                    .Set(f => f.NormalizedText, normalizedText)
                    .Set(f => f.Language, language)
                    .Set(f => f.SkillScores, skillScores)
                    .Set(f => f.UpdatedAt, DateTime.UtcNow);
                if (!string.IsNullOrEmpty(originalText))
                {
                    update = update.Set(f => f.OriginalText, originalText);
                }
                var result = await _files.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating processing results for file {FileId}", fileId);
                throw;
            }
        }


        public async Task<bool> AddJobAsync(string fileId, JobDocument job)
        {
            try
            {
                job.InitializeSteps();
                var filter = Builders<FileDocument>.Filter.Eq(f => f.FileId, fileId);
                var update = Builders<FileDocument>.Update
                    .Push(f => f.Jobs, job)
                    .Set(f => f.UpdatedAt, DateTime.UtcNow);

                var result = await _files.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding job for file {FileId}", fileId);
                throw;
            }
        }

        public async Task<bool> UpdateJobStatusAsync(string fileId, string jobId, JobStatus status)
        {
            try
            {
                var filter = Builders<FileDocument>.Filter.And(
                    Builders<FileDocument>.Filter.Eq(f => f.FileId, fileId),
                    Builders<FileDocument>.Filter.ElemMatch(f => f.Jobs, j => j.Id == jobId)
                );

                var update = Builders<FileDocument>.Update
                    .Set("Jobs.$.Status", status)
                    .Set(f => f.UpdatedAt, DateTime.UtcNow);

                if (status == JobStatus.InProgress)
                {
                    update = update.Set("Jobs.$.StartedAt", DateTime.UtcNow);
                }
                else if (status == JobStatus.Completed || status == JobStatus.Failed)
                {
                    update = update.Set("Jobs.$.CompletedAt", DateTime.UtcNow);
                }

                var result = await _files.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job status for file {FileId}, job {JobId}", fileId, jobId);
                throw;
            }
        }

        public async Task<bool> UpdateJobStepAsync(string fileId, string jobId, ProcessingStep step, bool isCompleted, string? errorMessage = null)
        {
            try
            {
                var filter = Builders<FileDocument>.Filter.And(
                    Builders<FileDocument>.Filter.Eq(f => f.FileId, fileId),
                    Builders<FileDocument>.Filter.ElemMatch(f => f.Jobs, j => j.Id == jobId)
                );

                var updateDefinition = Builders<FileDocument>.Update
                    .Set($"Jobs.$.Steps.{step}", new StepStatus
                    {
                        Step = step.ToString(),
                        IsCompleted = isCompleted,
                        CompletedAt = isCompleted ? DateTime.UtcNow : null,
                        StartedAt = DateTime.UtcNow,
                        ErrorMessage = errorMessage,
                    })
                    .Set("Jobs.$.CurrentStep", step)
                    .Set(f => f.UpdatedAt, DateTime.UtcNow);

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    updateDefinition = updateDefinition
                        .Set("Jobs.$.ErrorMessage", errorMessage)
                        .Set("Jobs.$.Status", JobStatus.Failed);
                }
                else
                {
                    updateDefinition = updateDefinition
                        .Set("Jobs.$.Status", JobStatus.InProgress);
                }
                var result = await _files.UpdateOneAsync(filter, updateDefinition);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating job step for file {FileId}, job {JobId}", fileId, jobId);
                throw;
            }
        }

        public async Task<bool> UpdateFileExtension(string fileId, string fileExtension, string? fileType = null)
        {
            try
            {
                var filter = Builders<FileDocument>.Filter.Eq(f => f.FileId, fileId);
                var update = Builders<FileDocument>.Update
                    .Set(f => f.FileExtension, Enum.Parse<FileExtension>(fileExtension));

                if (fileType != null)
                {
                    update.Set(f => f.FileType, Enum.Parse<FileType>(fileType));
                }

                var result = await _files.UpdateOneAsync(filter, update);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating metadata for file {FileId}", fileId);
                throw;
            }
        }
    }
}
