using Microsoft.AspNetCore.Antiforgery;
using UploadFiles.Services.Services.Upload.Models;
using UploadFiles.Services.Services.Upload;
using Microsoft.AspNetCore.Authorization;

namespace UploadFiles.RouteHandlers
{
    public static class UploadRoutes
    {
        public static void Configure(RouteGroupBuilder uploadMap)
        {
            uploadMap.MapPost("/", UploadFile);
            uploadMap.MapGet("/protected", [Authorize] () =>
            {
                return Results.Ok("You are authorized!");
            });
        }
        [Authorize]
        private static async Task<IResult> UploadFile(HttpContext httpContext, IFormFileCollection files, UploadManager uploadManager, IAntiforgery antiforgery)
        {
            try
            {
                await antiforgery.ValidateRequestAsync(httpContext);
                var user = httpContext.User;
                if (files == null || files.Count == 0)
                {
                    return Results.BadRequest("No file uploaded or file is empty.");
                }

                IList<FileUploadResult> fileUploadResults = new List<FileUploadResult>();

                foreach (var file in files)
                {
                    fileUploadResults.Add(await uploadManager.HandleUploadAsync(file));
                }

                return Results.Ok(fileUploadResults);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }
    }
}
