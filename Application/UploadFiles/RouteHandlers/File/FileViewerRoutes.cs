using System.Security.Claims;
using UploadFiles.DTOs;
using UploadFiles.Models;
using UploadFiles.Services.Interfaces;

namespace UploadFiles.RouteHandlers.File
{
    public class FileViewerRoutes : IFileViewerRoutes
    {
        private readonly IFileViewerService _fileService;
        private readonly ILogger<FileViewerRoutes> _logger;

        public FileViewerRoutes(
            IFileViewerService fileService,
            ILogger<FileViewerRoutes> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }


        public void Configure(RouteGroupBuilder group)
        {
            group.MapGet("/files", GetFilesAsync);
            group.MapGet("/files/{fileId}", GetFileDetailsAsync);
            group.MapGet("/files/search", SearchFilesAsync);
        }


        private async Task<IResult> GetFilesAsync(
              [AsParameters] FileQueryParameters queryParams,
              ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var result = await _fileService.GetFilesAsync(queryParams, userId);

                var response = new FileViewerResponse
                {
                    Files = result.Files,
                    Pagination = new PaginationMetadata
                    {
                        Page = queryParams.Page,
                        PageSize = queryParams.PageSize,
                        TotalCount = result.TotalCount,
                        TotalPages = (int)Math.Ceiling(result.TotalCount / (double)queryParams.PageSize),
                        HasNextPage = (queryParams.Page + 1) * queryParams.PageSize < result.TotalCount,
                        HasPreviousPage = queryParams.Page > 0
                    }
                };

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving files for user");
                return Results.Problem("Error retrieving files");
            }
        }

        private async Task<IResult> GetFileDetailsAsync(string fileId, ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var file = await _fileService.GetFileDetailsAsync(fileId, userId);
                return file != null ? Results.Ok(file) : Results.NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file details for {FileId}", fileId);
                return Results.Problem("Error retrieving file details");
            }
        }

        private async Task<IResult> SearchFilesAsync(
            [AsParameters] FileQueryParameters queryParams,
            ClaimsPrincipal user)
        {
            try
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var result = await _fileService.SearchFilesAsync(queryParams, userId);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching files");
                return Results.Problem("Error searching files");
            }
        }
    }
}
