using UploadFiles.RouteHandlers.Auth;
using UploadFiles.RouteHandlers.File;
using UploadFiles.RouteHandlers.Upload;

namespace UploadFiles.RouteHandlers
{
    public static class RouteConfigurator
    {
        public static void ConfigureRoutes(WebApplication app)
        {
            var authRoutes = app.MapGroup("/auth")
                .RequireAuthorization();
            app.ResolveRoutes<IAuthRoutes>(authRoutes);

            var uploadRoutes = app.MapGroup("/upload")
                .RequireAuthorization();
            app.ResolveRoutes<IUploadRoutes>(uploadRoutes);

            var statusRoutes = app.MapGroup("/status").RequireAuthorization();
            app.ResolveRoutes<IFileViewerRoutes>(statusRoutes);
        }

        private static void ResolveRoutes<T>(this WebApplication app, RouteGroupBuilder routeGroup)
            where T : IRouteConfiguration
        {
            using (var scope = app.Services.CreateScope())
            {
                var routes = scope.ServiceProvider.GetRequiredService<T>();
                routes.Configure(routeGroup);
            }
        }
    }
}
