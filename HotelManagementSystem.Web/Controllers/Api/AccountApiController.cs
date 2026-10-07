using HotelManagementSystem.Persistence.EfCore.Identity;
using HotelManagementSystem.Web.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Web.Controllers.Api;

public sealed record LoginRequest(string Email, string Password, bool RememberMe);

public sealed record AuthUserResponse(string Id, string Email, string FullName, string Role);

public sealed record LoginResponse(DateTime ExpiresAt, AuthUserResponse User);

[ApiController]
[Route("api/account")]
public sealed class AccountApiController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    JwtTokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Вкажіть email і пароль." });
        }

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized(new { message = "Невірний email або пароль." });
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            return Unauthorized(new { message = "Обліковий запис заблоковано." });
        }

        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Невірний email або пароль." });
        }

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;
        var (token, expiresAt) = tokenService.CreateToken(user, roles);

        Response.Cookies.Append(
            AuthCookie.Name,
            token,
            AuthCookie.Options(Request, request.RememberMe ? (DateTime?)expiresAt : null));

        return Ok(new LoginResponse(
            expiresAt,
            new AuthUserResponse(user.Id, user.Email ?? string.Empty, user.FullName, role)));
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookie.Name, AuthCookie.Options(Request, null));
        return NoContent();
    }

    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<AuthUserResponse>> Me()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new AuthUserResponse(user.Id, user.Email ?? string.Empty, user.FullName, roles.FirstOrDefault() ?? string.Empty));
    }
}