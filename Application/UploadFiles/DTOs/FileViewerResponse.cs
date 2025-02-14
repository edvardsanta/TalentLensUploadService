using UploadFiles.Models;

namespace UploadFiles.DTOs
{
    public class FileViewerResponse
    {
        public List<FileViewer> Files { get; set; }
        public PaginationMetadata Pagination { get; set; }
    }
}
