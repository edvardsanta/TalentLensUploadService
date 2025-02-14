using Microsoft.Extensions.Logging;
using UploadFiles.Infrastructure.Persistence.Repositories;
using UploadFiles.Models;
using UploadFiles.Services.Interfaces;
using UploadFiles.Services.Mapper;

namespace UploadFiles.Services.Services.Files
{
    public class FileViewerService : IFileViewerService
    {
        private readonly IFileSearchRepository _fileRepository;
        private readonly ILogger<FileViewerService> _logger;

        public FileViewerService(
            IFileSearchRepository fileRepository,
            ILogger<FileViewerService> logger)
        {
            _fileRepository = fileRepository;
            _logger = logger;
        }

        public async Task<(List<FileViewer> Files, int TotalCount)> GetFilesAsync(
            FileQueryParameters queryParams,
            string userId)
        {
            try
            {
                var (files, totalCount) = await _fileRepository.GetPaginatedFilesAsync(
                    userId,
                    queryParams.Page,
                    queryParams.PageSize,
                    queryParams.SortBy,
                    queryParams.Descending);

                var fileViewModels = files.Select(FileViewerServiceHelpers.MapToViewModel).ToList();
                return (fileViewModels, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated files");
                throw;
            }
        }

        public async Task<FileViewer> GetFileDetailsAsync(string fileId, string userId)
        {
            try
            {
                var file = await _fileRepository.GetByIdAsync(fileId);
                if (file == null) return null;

                return FileViewerServiceHelpers.MapToViewModel(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file details for {FileId}", fileId);
                throw;
            }
        }

        public async Task<(List<FileViewer> Files, int TotalCount)> SearchFilesAsync(
            FileQueryParameters queryParams,
            string userId)
        {
            try
            {
                var (files, totalCount) = await _fileRepository.SearchFilesAsync(
                    userId,
                    queryParams.SearchTerm,
                    queryParams.Status,
                    queryParams.StartDate,
                    queryParams.EndDate,
                    queryParams.Page,
                    queryParams.PageSize);

                var fileViewModels = files.Select(FileViewerServiceHelpers.MapToViewModel).ToList();
                return (fileViewModels, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching files");
                throw;
            }
        }
    }
}
