using Microsoft.AspNetCore.Antiforgery;

namespace UploadFiles.RouteHandlers.Auth
{
    public class AuthRoutes : IAuthRoutes
    {
        private readonly IAntiforgery _antiforgery;

        public AuthRoutes(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }
        public void Configure(RouteGroupBuilder authRoutes)
        {
            authRoutes.MapGet("/antiforgerytoken", GetAntiforgeryToken).RequireAuthorization();
        }

        public IResult GetAntiforgeryToken(HttpContext httpContext)
        {
            var tokens = _antiforgery.GetAndStoreTokens(httpContext);
            httpContext.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions { HttpOnly = false });
            return Results.Ok(new { token = tokens.RequestToken });
        }
    }
}
