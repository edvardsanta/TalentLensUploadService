using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UploadFiles.Services.Services.Upload.Interfaces;
using UploadFiles.Services.Services.Upload.Models;

namespace UploadFiles.RouteHandlers.Upload
{
    public class UploadRoutes : IUploadRoutes
    {
        public void Configure(RouteGroupBuilder uploadMap)
        {
            uploadMap.MapPost("/", UploadFile);
            uploadMap.MapGet("/protected", [Authorize] () =>
            {
                return Results.Ok("You are authorized!");
            });
        }

        private async Task<IResult> UploadFile(HttpContext httpContext, IFormFileCollection files, IFileProcessingService processingService, IAntiforgery antiforgery, ClaimsPrincipal user)
        {
            try
            {
                await antiforgery.ValidateRequestAsync(httpContext);

                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }
                if (files == null || files.Count == 0)
                {
                    return Results.BadRequest("No file uploaded or file is empty.");
                }

                IList<FileUploadResult> results = new List<FileUploadResult>();

                foreach (var file in files)
                {
                    results.Add(await processingService.ProcessFileUploadAsync(file, userId));
                }

                return Results.Ok(results);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }
        }
    }
}
