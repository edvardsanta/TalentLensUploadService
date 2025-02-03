using UploadFiles.Configurations;
using UploadFiles.Handlers;
using UploadFiles.Middleware;

var builder = WebApplication.CreateSlimBuilder(args);

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

ServicesConfiguration.ConfigureServices(builder.Services, config);
builder.Services.AddAntiforgery(AntiforgeryConfiguration.ConfigureAntiforgery);
ServicesConfiguration.ConfigureAuthentication(builder.Services, config);
builder.Services.AddAuthorization();
builder.Services.AddCors(CorsConfiguration.ConfigureCors);


WebApplication app = builder.Build();

app.UseCors("AllowSpecificOrigin"); 
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120),
});
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.UseMiddleware<WebSocketMiddleware>();

RouteConfigurator.ConfigureRoutes(app);

// Run the app
app.Run();