using UploadFiles.Models;

namespace UploadFiles.Services.Interfaces
{
    public interface IFileViewerService
    {
        Task<(List<FileViewer> Files, int TotalCount)> GetFilesAsync(
            FileQueryParameters queryParams,
            string userId);

        Task<FileViewer> GetFileDetailsAsync(
            string fileId,
            string userId);

        Task<(List<FileViewer> Files, int TotalCount)> SearchFilesAsync(
            FileQueryParameters queryParams,
            string userId);
    }
}
