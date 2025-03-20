using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using UploadFiles.Infrastructure.Config.Settings;
using UploadFiles.Infrastructure.Persistence.Repositories;

namespace UploadFiles.Infrastructure.Config
{
    public static class InfrastructureConfiguration
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .AddMongoDb(configuration)
                .AddRepository();

            return services;
        }

        private static IServiceCollection AddMongoDb(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var mongoSettings = configuration.GetSection("MongoDb").Get<MongoDbSettings>()
                ?? throw new InvalidOperationException("MongoDB settings not found");

            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = MongoClientSettings.FromConnectionString(mongoSettings.ConnectionString);
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);
                if (!string.IsNullOrEmpty(mongoSettings.Username) && !string.IsNullOrEmpty(mongoSettings.Password))
                {
                    settings.Credential = MongoCredential.CreateCredential(
                        mongoSettings.AuthDatabase ?? "admin",
                        mongoSettings.Username,
                        mongoSettings.Password);
                }
                return new MongoClient(settings);
            });

            services.AddScoped(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase(mongoSettings.DatabaseName);
            });

            return services;
        }

        private static IServiceCollection AddRepository(
           this IServiceCollection services)
        {
            services.AddScoped<IFileRepository, FileRepository>();
            services.AddScoped<IFileSearchRepository, FileSearchRepository>();

            return services;
        }
    }
}
