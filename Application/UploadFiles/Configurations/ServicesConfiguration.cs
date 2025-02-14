using MassTransit;
using Microsoft.AspNetCore.Http.Features;
using System.Collections.Immutable;
using UploadFiles.Services.Interfaces;
using UploadFiles.Services.Services.Files;
using UploadFiles.Services.Services.Upload;
using UploadFiles.Services.Services.Upload.Interfaces;
namespace UploadFiles.Configurations
{
    public static class ServicesConfiguration
    {
        private static ImmutableList<IFileHandler> ConfigureFileHandlers() => [
                new PDFUpload(),
                new XLSXUpload()
        ];

        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 209715200;
            });

            services.AddScoped(_ => ConfigureFileHandlers());
            services.AddScoped<IFileProcessingService, FileProcessingService>();
            services.AddScoped<IFileViewerService, FileViewerService>();
            services.AddScoped<UploadManager>();
            return services;
        }


        public static IServiceCollection ConfigureMassTransit(this IServiceCollection services, IConfiguration config)
        {
            var rabbitMqUri = config["RabbitMq:Uri"];
            var rabbitMqUser = config["RabbitMq:Username"];
            var rabbitMqPass = config["RabbitMq:Password"];

            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqUri, "/", h =>
                    {
                        h.Username(rabbitMqUser);
                        h.Password(rabbitMqPass);
                    });
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }

}
