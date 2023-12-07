using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using UploadFiles;
using UploadFiles.Interfaces;
using UploadFiles.Services;

var builder = WebApplication.CreateSlimBuilder(args);
ImmutableList<IFileHandler> handlers = Configuration.ConfigureFileHandlers();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 209715200;
});
builder.Services.AddScoped<UploadManager>(x => new UploadManager(handlers));
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});
WebApplication app = builder.Build();
RouteGroupBuilder uploadMap = app.MapGroup("/upload");
uploadMap.MapPost("/", async (IFormFile file, UploadManager uploadManager) =>
{
    try
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest("No file uploaded or file is empty.");
        }
        await uploadManager.HandleUploadAsync(file);

        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex);
    }
}).DisableAntiforgery();
app.Run();

[JsonSerializable(typeof(string))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}