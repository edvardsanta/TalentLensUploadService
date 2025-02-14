using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using UploadFiles.Shared.Models.File;

namespace UploadFiles.Infrastructure.Persistence.Repositories
{
    public class BaseFileRepository
    {
        protected readonly IMongoCollection<FileDocument> _files;
        protected readonly ILogger _logger;

        public BaseFileRepository(
            IMongoDatabase database,
            ILogger logger)
        {
            _files = database.GetCollection<FileDocument>("files");
            _logger = logger;
        }
    }
}
