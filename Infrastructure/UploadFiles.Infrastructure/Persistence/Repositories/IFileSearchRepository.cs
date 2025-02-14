using MongoDB.Bson;
using UploadFiles.Shared.Enums;
using UploadFiles.Shared.Models.File;

namespace UploadFiles.Infrastructure.Persistence.Repositories
{
    public interface IFileSearchRepository
    {
        Task<(List<FileDocument> Files, int TotalCount)> SearchFilesAsync(
            string userId,
            string searchTerm,
            string status,
            DateTime? startDate,
            DateTime? endDate,
            int page,
            int pageSize);
        Task<(List<FileDocument> Files, int TotalCount)> GetPaginatedFilesAsync(
            string userId,
            int page,
            int pageSize,
            string sortBy,
            bool descending);
        Task<List<FileDocument>> GetFilesBySkillScoreAsync(string skill, double minScore, int limit = 10);
        Task<List<FileDocument>> GetFilesByLanguageAsync(string language, int page = 1, int pageSize = 10);
        Task<Dictionary<string, int>> GetFileTypeDistributionAsync(string userId);

        Task<FileDocument> GetByIdAsync(string fileId);

        Task<FileDocument> GetByObjectIdAsync(ObjectId id);

        Task<IEnumerable<FileDocument>> GetFilesByTypeAsync(FileType fileType);
        Task<IEnumerable<FileDocument>> GetRecentFilesAsync(int limit = 100);
    }
}
