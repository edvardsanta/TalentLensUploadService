using Microsoft.AspNetCore.Http.Features;
using System.Collections.Immutable;
using UploadFiles;
using UploadFiles.Interfaces;
using UploadFiles.Services;

var builder = WebApplication.CreateSlimBuilder(args);
ImmutableList<IFileHandler> handlers = Configuration.ConfigureFileHandlers();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 209715200;
});
//builder.Services.ConfigureHttpJsonOptions();
WebApplication app = builder.Build();
RouteGroupBuilder uploadMap = app.MapGroup("/upload");
uploadMap.MapPost("/", async (IFormFile file) =>
{
    try
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest("No file uploaded or file is empty.");
        }
        await new UploadManager(handlers).HandleUploadAsync(file);

        return Results.Ok("File uploaded successfully.");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex);
    }
}).DisableAntiforgery();
app.Run();