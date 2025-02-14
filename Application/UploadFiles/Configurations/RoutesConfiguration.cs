using UploadFiles.RouteHandlers.Auth;
using UploadFiles.RouteHandlers.File;
using UploadFiles.RouteHandlers.Upload;

namespace UploadFiles.Configurations
{
    public static class RoutesConfiguration
    {
        public static IServiceCollection AddRoutes(this IServiceCollection services)
        {
            services.AddScoped<IAuthRoutes, AuthRoutes>();
            services.AddScoped<IUploadRoutes, UploadRoutes>();
            services.AddScoped<IFileViewerRoutes, FileViewerRoutes>();

            return services;
        }
    }
}
