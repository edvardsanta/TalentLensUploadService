using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using UploadFiles.Shared.Enums;
using UploadFiles.Shared.Models.File;

namespace UploadFiles.Infrastructure.Persistence.Repositories
{
    public class FileSearchRepository : BaseFileRepository, IFileSearchRepository
    {
        public FileSearchRepository(
           IMongoDatabase database,
           ILogger<FileSearchRepository> logger) : base(database, logger)
        {
        }

        public async Task<(List<FileDocument> Files, int TotalCount)> SearchFilesAsync(
            string userId,
            string searchTerm,
            string status,
            DateTime? startDate,
            DateTime? endDate,
            int page,
            int pageSize)
        {
            try
            {
                var filterBuilder = Builders<FileDocument>.Filter;
                var filters = new List<FilterDefinition<FileDocument>>
                {
                    filterBuilder.Eq(f => f.UserId, userId)
                };

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchRegex = new BsonRegularExpression(searchTerm, "i");
                    filters.Add(filterBuilder.Or(
                        filterBuilder.Regex(f => f.FileName, searchRegex),
                        filterBuilder.Regex(f => f.NormalizedText, searchRegex),
                        filterBuilder.Regex(f => f.OriginalText, searchRegex)
                    ));
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    filters.Add(filterBuilder.Eq("CurrentJob.Status", status));
                }

                if (startDate.HasValue)
                {
                    filters.Add(filterBuilder.Gte(f => f.CreatedAt, startDate.Value));
                }
                if (endDate.HasValue)
                {
                    filters.Add(filterBuilder.Lte(f => f.CreatedAt, endDate.Value));
                }

                var combinedFilter = filterBuilder.And(filters);

                var totalCount = await _files.CountDocumentsAsync(combinedFilter);

                var files = await _files.Find(combinedFilter)
                    .Sort(Builders<FileDocument>.Sort.Descending(f => f.CreatedAt))
                    .Skip((page - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                return (files, (int)totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching files for user {UserId}", userId);
                throw;
            }
        }

        public async Task<(List<FileDocument> Files, int TotalCount)> GetPaginatedFilesAsync(
            string userId,
            int page,
            int pageSize,
            string sortBy,
            bool descending)
        {
            try
            {
                page = Math.Max(1, page);
                pageSize = Math.Max(1, Math.Min(pageSize, 100)); 
                var filter = Builders<FileDocument>.Filter.Eq(f => f.UserId, userId);
                var sortDefinition = CreateSortDefinition(sortBy, descending);

                var totalCount = await _files.CountDocumentsAsync(filter);

                var skip = (page - 1) * pageSize;

                var files = await _files.Find(filter)
                    .Sort(sortDefinition)
                    .Skip(skip)
                    .Limit(pageSize)
                    .ToListAsync();

                return (files, (int)totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated files for user {UserId}", userId);
                throw;
            }
        }

        private SortDefinition<FileDocument> CreateSortDefinition(string sortBy, bool descending)
        {
            var sortBuilder = Builders<FileDocument>.Sort;
            return sortBy?.ToLower() switch
            {
                "filename" => descending
                    ? sortBuilder.Descending(f => f.FileName)
                    : sortBuilder.Ascending(f => f.FileName),
                "createdat" => descending
                    ? sortBuilder.Descending(f => f.CreatedAt)
                    : sortBuilder.Ascending(f => f.CreatedAt),
                "filesize" => descending
                    ? sortBuilder.Descending(f => f.FileSize)
                    : sortBuilder.Ascending(f => f.FileSize),
                _ => descending
                    ? sortBuilder.Descending(f => f.CreatedAt)
                    : sortBuilder.Ascending(f => f.CreatedAt)
            };
        }

        public Task<List<FileDocument>> GetFilesBySkillScoreAsync(string skill, double minScore, int limit = 10)
        {
            throw new NotImplementedException();
        }

        public Task<List<FileDocument>> GetFilesByLanguageAsync(string language, int page = 1, int pageSize = 10)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, int>> GetFileTypeDistributionAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<FileDocument> GetByIdAsync(string fileId)
        {
            try
            {
                var filter = Builders<FileDocument>.Filter.Eq(f => f.FileId, fileId);
                return await _files.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file {FileId}", fileId);
                throw;
            }
        }

        public async Task<FileDocument> GetByObjectIdAsync(ObjectId id)
        {
            try
            {
                var filter = Builders<FileDocument>.Filter.Eq(f => f.Id, id);
                return await _files.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file by ObjectId {Id}", id);
                throw;
            }
        }


        public async Task<IEnumerable<FileDocument>> GetFilesByTypeAsync(FileType fileType)
        {
            try
            {
                var filter = Builders<FileDocument>.Filter.Eq(f => f.FileType, fileType);
                return await _files.Find(filter)
                    .Sort(Builders<FileDocument>.Sort.Descending(f => f.CreatedAt))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting files by type {FileType}", fileType);
                throw;
            }
        }

        public async Task<IEnumerable<FileDocument>> GetRecentFilesAsync(int limit = 100)
        {
            try
            {
                return await _files.Find(FilterDefinition<FileDocument>.Empty)
                    .Sort(Builders<FileDocument>.Sort.Descending(f => f.CreatedAt))
                    .Limit(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent files");
                throw;
            }
        }
    }
}
