namespace HotelManagementSystem.Web.Auth;

public static class AuthCookie
{
    public const string Name = "hms_auth";

    public static CookieOptions Options(HttpRequest request, DateTime? expiresAtUtc) => new()
    {
        HttpOnly = true,                       
        Secure = request.IsHttps,              
        SameSite = SameSiteMode.Strict,        
        Path = "/",
        Expires = expiresAtUtc is null
            ? null                             
            : new DateTimeOffset(expiresAtUtc.Value),
    };
}