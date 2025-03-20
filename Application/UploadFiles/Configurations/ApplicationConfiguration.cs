using UploadFiles.Extensions;
using UploadFiles.RouteHandlers;
using UploadFiles.Infrastructure.Config;
using UploadFiles.Middleware;

namespace UploadFiles.Configurations
{
    public static partial class ApplicationConfiguration
    {
        public record ApplicationState
        {
            public IConfiguration Configuration { get; init; }
            public WebApplicationBuilder Builder { get; init; }
            public WebApplication App { get; init; }

            private ApplicationState(IConfiguration config, WebApplicationBuilder builder, WebApplication app = null)
            {
                Configuration = config;
                Builder = builder;
                App = app;
            }

            public static Result<ApplicationState> Create(string[] args)
            {
                try
                {
                    var config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: true)
                        .AddEnvironmentVariables()
                        .Build();

                    var builder = WebApplication.CreateSlimBuilder(args);

                    return Result<ApplicationState>.Success(new ApplicationState(config, builder));
                }
                catch (Exception ex)
                {
                    return Result<ApplicationState>.Failure($"Failed to create application state: {ex.Message}");
                }
            }
        }

        public static class ConfigurationFunctions
        {
            public static Func<ApplicationState, Result<ApplicationState>> ConfigureServices =>
                state => Result<ApplicationState>.Success(state with
                {
                    Builder = state.Builder.Apply(builder =>
                    {
                        builder.Services
                            .AddLogging()
                            .AddInfrastructureServices(state.Configuration)
                            .AddMongoCollections()
                            .ConfigureServices(state.Configuration)
                            .ConfigureMassTransit(state.Configuration)
                            .AddAntiforgery(AntiforgeryConfiguration.ConfigureAntiforgery)
                            .ConfigureAuthentication(state.Configuration)
                            .AddAuthorization()
                            .AddCors(CorsConfiguration.ConfigureCors)
                            .AddRoutes();
                    })
                });

            public static Func<ApplicationState, Result<ApplicationState>> BuildApplication =>
                state => Result<ApplicationState>.Success(state with { App = state.Builder.Build() });

            public static Func<ApplicationState, Result<ApplicationState>> ConfigureMiddleware =>
                state => Result<ApplicationState>.Success(state with
                {
                    App = state.App.Apply(app =>
                    {
                        app.UseCors("AllowSpecificOrigin")
                           .UseWebSockets(new WebSocketOptions
                           {
                               KeepAliveInterval = TimeSpan.FromSeconds(120)
                           })
                           .UseAuthentication()
                           .UseAuthorization()
                           .UseAntiforgery()
                           .UseMiddleware<WebSocketMiddleware>();

                        RouteConfigurator.ConfigureRoutes(app);
                    })
                });

            public static async Task<Result<ApplicationState>> InitializeDatabase(ApplicationState state)
            {
                try
                {
                    await state.App.Services.InitializeMongoDbCollectionsAsync();
                    return Result<ApplicationState>.Success(state);
                }
                catch (Exception ex)
                {
                    return Result<ApplicationState>.Failure($"Database initialization failed: {ex.Message}");
                }
            }
        }
    }
}
