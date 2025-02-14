using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using UploadFiles.Shared.Enums;
using UploadFiles.Shared.Models.Job;

namespace UploadFiles.Shared.Models.File
{
    public class FileDocument
    {
        public ObjectId Id { get; set; }

        public string UserId { get; set; }
        public string FileId { get; set; } = Guid.NewGuid().ToString();
        public FileType FileType { get; set; }
        public FileExtension FileExtension { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string StoragePath { get; set; }
        public string OriginalText { get; set; }
        public string NormalizedText { get; set; }
        public string Language { get; set; }
        public Dictionary<string, double> SkillScores { get; set; } = new();

        public List<JobDocument> Jobs { get; set; } = new();
        public JobDocument CurrentJob => Jobs.OrderByDescending(j => j.StartedAt).FirstOrDefault();

        [BsonIgnoreIfNull]
        public Dictionary<string, object> FileMetadata { get; set; } = new();
    }
}
