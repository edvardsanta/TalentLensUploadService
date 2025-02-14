using MongoDB.Driver;
using UploadFiles.Infrastructure.Config.Settings;
using UploadFiles.Shared.Models.File;

namespace UploadFiles.Configurations
{
    public static class RepositoryConfiguration
    {
        public static IServiceCollection AddMongoCollections(this IServiceCollection services)
        {
            services.AddSingleton<MongoDBCollections>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                return new MongoDBCollections
                {
                    FileCollectionName = config["MongoDB:Collections:Files"] ?? "files",
                };
            });

            return services;
        }

        public static async Task InitializeMongoDbCollectionsAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
            var dbConfig = scope.ServiceProvider.GetRequiredService<MongoDBCollections>();

            var collections = await (await database.ListCollectionNamesAsync()).ToListAsync();

            if (!collections.Contains(dbConfig.FileCollectionName))
            {
                await database.CreateCollectionAsync(dbConfig.FileCollectionName);
                var fileCollection = database.GetCollection<FileDocument>(dbConfig.FileCollectionName);

                var fileIndexes = new[]
                {
                    new CreateIndexModel<FileDocument>(
                        Builders<FileDocument>.IndexKeys.Ascending(f => f.FileId),
                        new CreateIndexOptions { Unique = true }),
                    new CreateIndexModel<FileDocument>(
                        Builders<FileDocument>.IndexKeys.Ascending(f => f.FileType)),
                    new CreateIndexModel<FileDocument>(
                        Builders<FileDocument>.IndexKeys.Descending(f => f.CreatedAt))
                };

                await fileCollection.Indexes.CreateManyAsync(fileIndexes);
            }
        }
    }
}
