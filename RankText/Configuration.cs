using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RankText.Interfaces;
using UploadFiles.Infrastructure.Config;


namespace RankText
{
    public static class Configuration
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {

            var configuration = hostContext.Configuration;

            var rabbitMqUri = configuration["RabbitMq:Uri"];
            var queueName = configuration["RabbitMq:QueueName"];
            var rabbitMqUser = configuration["RabbitMq:Username"];
            var rabbitMqPass = configuration["RabbitMq:Password"];
            if (string.IsNullOrEmpty(queueName))
            {
                throw new InvalidOperationException("RabbitMQ Queue Name is not configured.");
            }
            services.AddMassTransit(x =>
            {
                x.AddConsumer<RankTextConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqUri, "/", h =>
                    {
                        h.Username(rabbitMqUser);
                        h.Password(rabbitMqPass);
                    });
                    cfg.ReceiveEndpoint(queueName, e =>
                    {
                        e.ConfigureConsumer<RankTextConsumer>(context);
                    });
                });
            });

            services.AddInfrastructureServices(configuration);
            services.AddScoped<ITextClassificationService, TextClassificationService>();
        }
        public static void ConfigureAppConfiguration(HostBuilderContext hostContext, IConfigurationBuilder config)
        {
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        }

        public static void ConfigureLogging(HostBuilderContext hostContext, ILoggingBuilder logging)
        {
            logging.AddConsole();
        }
    }
}
