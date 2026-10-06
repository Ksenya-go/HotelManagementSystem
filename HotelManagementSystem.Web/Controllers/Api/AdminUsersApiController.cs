using HotelManagementSystem.Persistence.EfCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Web.Controllers.Api;

public sealed record UserRowResponse(string Id, string Email, string FullName, string Role, bool IsLocked);

public sealed record UserDetailsResponse(string Id, string FullName, string Email, string Role);

public sealed record CreateUserRequest(string FullName, string Email, string Password, string Role);

public sealed record UpdateUserRequest(string FullName, string Email, string Role, string? NewPassword);

public sealed record ChangeRoleRequest(string Role);

[ApiController]
[Route("api/admin/users")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public sealed class AdminUsersApiController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : ControllerBase
{
    private const string EmployeeRole = "Employee";
    private const string AdminRole = "Admin";

    // GET /api/admin/users
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserRowResponse>>> List()
    {
        var users = await userManager.Users.OrderBy(u => u.Email).ToListAsync();
        var rows = new List<UserRowResponse>(users.Count);

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            rows.Add(new UserRowResponse(
                user.Id,
                user.Email ?? user.UserName ?? string.Empty,
                user.FullName,
                roles.FirstOrDefault() ?? string.Empty,
                user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow));
        }

        return Ok(rows);
    }

    // GET /api/admin/users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDetailsResponse>> GetById(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new UserDetailsResponse(
            user.Id, user.FullName, user.Email ?? string.Empty, roles.FirstOrDefault() ?? string.Empty));
    }

    // POST /api/admin/users
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var availableRoles = await GetAssignableRolesAsync();
        if (!availableRoles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Оберіть доступну роль." });
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            EmailConfirmed = true,
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(new { message = MapIdentityError(createResult) });
        }

        var roleResult = await userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return BadRequest(new { message = MapIdentityError(roleResult) });
        }

        return NoContent();
    }

    // PUT /api/admin/users/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request)
    {
        var availableRoles = await GetAssignableRolesAsync();
        if (!availableRoles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Оберіть доступну роль." });
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.UserName = request.Email;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return BadRequest(new { message = MapIdentityError(updateResult) });
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);
        await userManager.AddToRoleAsync(user, request.Role);

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);
            if (!resetResult.Succeeded)
            {
                return BadRequest(new { message = MapIdentityError(resetResult) });
            }
        }

        return NoContent();
    }

    // POST /api/admin/users/{id}/toggle-lockout
    [HttpPost("{id}/toggle-lockout")]
    public async Task<IActionResult> ToggleLockout(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        if (user.Id == userManager.GetUserId(User))
        {
            return BadRequest(new { message = "Не можна заблокувати власний обліковий запис." });
        }

        user.LockoutEnd = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
            ? null
            : DateTimeOffset.UtcNow.AddYears(100);

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = MapIdentityError(result) });
        }

        return NoContent();
    }

    // POST /api/admin/users/{id}/change-role
    [HttpPost("{id}/change-role")]
    public async Task<IActionResult> ChangeRole(string id, [FromBody] ChangeRoleRequest request)
    {
        var availableRoles = await GetAssignableRolesAsync();
        if (!availableRoles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Оберіть доступну роль." });
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var currentRoles = await userManager.GetRolesAsync(user);
        var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            return BadRequest(new { message = MapIdentityError(removeResult) });
        }

        var addResult = await userManager.AddToRoleAsync(user, request.Role);
        if (!addResult.Succeeded)
        {
            return BadRequest(new { message = MapIdentityError(addResult) });
        }

        return NoContent();
    }

    private async Task<List<string>> GetAssignableRolesAsync()
    {
        return await roleManager.Roles
            .Where(r => r.Name == EmployeeRole || r.Name == AdminRole)
            .OrderBy(r => r.Name == EmployeeRole ? 0 : 1)
            .Select(r => r.Name!)
            .ToListAsync();
    }

    private static string MapIdentityError(IdentityResult result)
    {
        var messages = result.Errors.Select(e => e.Code switch
        {
            "DuplicateUserName" or "DuplicateEmail" => "Користувач із такою поштою вже існує.",
            "InvalidEmail" => "Вкажіть коректну електронну пошту.",
            "PasswordTooShort" or "PasswordRequiresNonAlphanumeric" or
                "PasswordRequiresDigit" or "PasswordRequiresLower" or
                "PasswordRequiresUpper" => "Пароль не відповідає вимогам безпеки.",
            _ => "Не вдалося виконати операцію з користувачем.",
        }).Distinct();

        return string.Join(" ", messages);
    }
}