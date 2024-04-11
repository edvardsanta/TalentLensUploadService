using Microsoft.Extensions.Hosting;
using static TextConsumer.Configuration;

IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(ConfigureLogging)
                .Build();

host.Run();