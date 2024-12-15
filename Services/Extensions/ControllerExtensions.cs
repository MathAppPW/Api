using Microsoft.AspNetCore.Mvc;

namespace Api.Services.Extensions;

public static class ControllerExtensions
{
    public static void SetCookie(this ControllerBase controller, string name, string value, DateTimeOffset expires,
        bool isHttpOnly = true, bool isSecure = true, string path = "/")
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = isHttpOnly,
            Secure = isSecure,
            SameSite = SameSiteMode.Strict,
            Expires = expires,
            Path = path
        };

        controller.Response.Cookies.Append(name, value, cookieOptions);
    }
}