using Microsoft.AspNetCore.Antiforgery;

namespace UploadFiles.RouteHandlers
{
    public static class AntiforgeryHandler
    {
        public static IResult GetAntiforgeryToken(IAntiforgery antiforgery, HttpContext httpContext)
        {
            var tokens = antiforgery.GetAndStoreTokens(httpContext);
            httpContext.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions { HttpOnly = false });
            return Results.Ok(new { token = tokens.RequestToken });
        }

    }
}
