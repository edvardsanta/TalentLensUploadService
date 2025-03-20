using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TextConsumer.Services;
using TextConsumer.Services.Interfaces;
using UploadFiles.Infrastructure.Config;


namespace TextConsumer
{
    public static class Configuration
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            var configuration = hostContext.Configuration;

            var rabbitMqUri = configuration["RabbitMq:Uri"];
            var consumeQueueName = configuration["RabbitMq:ProcessTextQueue"];
            var targetQueueName = configuration["RabbitMq:RankTextQueue"];
            var rabbitMqUser = configuration["RabbitMq:Username"];
            var rabbitMqPass = configuration["RabbitMq:Password"];
            if (string.IsNullOrEmpty(consumeQueueName))
            {
                throw new InvalidOperationException("RabbitMQ Queue Name is not configured.");
            }
            services.AddMassTransit(x =>
            {
                x.AddConsumer<TextProcessingConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqUri, "/", h =>
                    {
                        h.Username(rabbitMqUser);
                        h.Password(rabbitMqPass);
                    });

                    cfg.ReceiveEndpoint(consumeQueueName, e =>
                    {
                        e.ConfigureConsumer<TextProcessingConsumer>(context);
                    });
                });
            });
            services.AddSingleton<IConfiguration>(hostContext.Configuration);

            services.AddInfrastructureServices(configuration);
            services.AddScoped<ITextProcessingService, TextProcessingService>();
        }

        public static void ConfigureAppConfiguration(HostBuilderContext hostContext, IConfigurationBuilder config)
        {
            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();
        }

        public static void ConfigureLogging(HostBuilderContext hostContext, ILoggingBuilder logging)
        {
            logging.AddConsole();
        }
    }
}
