using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace RankText
{
    public static class Configuration
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {

            var configuration = hostContext.Configuration;

            var rabbitMqUri = configuration["RabbitMq:Uri"];
            var queueName = configuration["RabbitMq:QueueName"];
            if (string.IsNullOrEmpty(queueName))
            {
                throw new InvalidOperationException("RabbitMQ Queue Name is not configured.");
            }
            services.AddMassTransit(x =>
            {
                x.AddConsumer<RankTextConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitMqUri);

                    cfg.ReceiveEndpoint(queueName, e =>
                    {
                        e.ConfigureConsumer<RankTextConsumer>(context);
                    });
                });
            });
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
