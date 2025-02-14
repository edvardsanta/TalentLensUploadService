namespace UploadFiles.Infrastructure.Config.Settings
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public MongoDBCollections Collections { get; set; }
        public string? Username { get; set; }
        public string? AuthDatabase { get; set; }
        public string Password { get; set; }
    }
}
