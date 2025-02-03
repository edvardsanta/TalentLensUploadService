using UploadFiles.RouteHandlers;

namespace UploadFiles.Handlers
{
    public static class RouteConfigurator
    {
        public static void ConfigureRoutes(WebApplication app)
        {
            app.MapGet("/auth/antiforgerytoken", AntiforgeryHandler.GetAntiforgeryToken).RequireAuthorization();

            var uploadMap = app.MapGroup("/upload");
            UploadRoutes.Configure(uploadMap) ;

            var statusMap = app.MapGroup("/status");
            StatusRoutes.Configure(statusMap);
        }
    }
}
