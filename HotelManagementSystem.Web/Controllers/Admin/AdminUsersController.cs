using HotelManagementSystem.Persistence.EfCore.Identity;
using HotelManagementSystem.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using HotelManagementSystem.Web.Extensions;

namespace HotelManagementSystem.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/Users")]
public sealed class AdminUsersController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IStringLocalizer<SharedResource> sharedLocalizer) : Controller
{
    private const string EmployeeRole = "Employee";
    private const string AdminRole = "Admin";

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var users = await userManager.Users
            .OrderBy(user => user.Email)
            .ToListAsync();
        var rows = new List<UserRowViewModel>(users.Count);

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            rows.Add(new UserRowViewModel(
                user.Id,
                user.Email ?? user.UserName ?? string.Empty,
                user.FullName,
                roles.FirstOrDefault() ?? string.Empty,
                user.LockoutEnd.HasValue &&
                user.LockoutEnd.Value > DateTimeOffset.UtcNow));
        }

        return View("~/Views/Admin/Users/Index.cshtml", new AdminUsersViewModel { Users = rows });
    }

    [HttpGet("{id}/Edit")]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await userManager.FindByIdAsync(id);

        if (user is null) return NotFound();

        var roles = await userManager.GetRolesAsync(user);
        var model = new EditEmployeeViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? string.Empty,
            AvailableRoles = await GetAssignableRolesAsync()
        };

        return View("~/Views/Admin/Users/Edit.cshtml", model);
    }

    [HttpPost("{id}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditEmployeeViewModel model)
    {
        var availableRoles = await GetAssignableRolesAsync();
        model.AvailableRoles = availableRoles;

        AddInvalidRoleError(model.Role, availableRoles);

        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/Users/Edit.cshtml", model);
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }
        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            AddIdentityErrors(updateResult);
            return View("~/Views/Admin/Users/Edit.cshtml", model);
        }
        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);
        await userManager.AddToRoleAsync(user, model.Role);

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            var passwordResult = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await userManager.ResetPasswordAsync(user, passwordResult, model.NewPassword);
            if (!resetResult.Succeeded)
            {
                AddIdentityErrors(resetResult);
                return View("~/Views/Admin/Users/Edit.cshtml", model);
            }
        }

        TempData.SetSuccessMessage(
            sharedLocalizer["EmployeeUpdateSuccess"]);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var model = new CreateEmployeeViewModel
        {
            AvailableRoles = await GetAssignableRolesAsync()
        };

        return View("~/Views/Admin/Users/Create.cshtml", model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeViewModel model)
    {
        var availableRoles = await GetAssignableRolesAsync();
        model.AvailableRoles = availableRoles;

        AddInvalidRoleError(model.Role, availableRoles);

        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/Users/Create.cshtml", model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            return View("~/Views/Admin/Users/Create.cshtml", model);
        }

        var roleResult = await userManager.AddToRoleAsync(user, model.Role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            AddIdentityErrors(roleResult);
            return View("~/Views/Admin/Users/Create.cshtml", model);
        }

        TempData.SetSuccessMessage(
            sharedLocalizer["EmployeeCreateSuccess"]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id}/ToggleLockout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLockout(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        if (user.Id == userManager.GetUserId(User))
        {
            TempData.SetErrorMessage(
                sharedLocalizer["SelfLockoutError"]);

            return RedirectToAction(nameof(Index));
        }

        user.LockoutEnd = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow
            ? null
            : DateTimeOffset.UtcNow.AddYears(100);
        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            AddIdentityErrorsToTempData(result);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id}/ChangeRole")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(string id, string role)
    {
        var availableRoles = await GetAssignableRolesAsync();

        if (!availableRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            TempData.SetErrorMessage(
                sharedLocalizer["InvalidRoleError"]);

            return RedirectToAction(nameof(Index));
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            AddIdentityErrorsToTempData(removeResult);
            return RedirectToAction(nameof(Index));
        }

        var result = await userManager.AddToRoleAsync(user, role);

        if (!result.Succeeded)
        {
            AddIdentityErrorsToTempData(result);
        }

        return RedirectToAction(nameof(Index));
    }

    private void AddInvalidRoleError(
        string role,
        IReadOnlyCollection<string> availableRoles)
    {
        if (!availableRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                nameof(EditEmployeeViewModel.Role),
                sharedLocalizer["InvalidRoleError"]);
        }
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(
                string.Empty,
                GetIdentityErrorMessage(error));
        }
    }

    private void AddIdentityErrorsToTempData(IdentityResult result)
    {
        var messages = result.Errors
            .Select(GetIdentityErrorMessage)
            .Distinct()
            .ToList();

        TempData.SetErrorMessage(
            string.Join(" ", messages));
    }

    private string GetIdentityErrorMessage(
        IdentityError error)
    {
        return error.Code switch
        {
            "DuplicateUserName" or "DuplicateEmail" =>
                sharedLocalizer["UserAlreadyExistsError"].Value,

            "InvalidEmail" =>
                sharedLocalizer["EmailInvalid"].Value,

            "PasswordTooShort" or "PasswordRequiresNonAlphanumeric" or
                "PasswordRequiresDigit" or "PasswordRequiresLower" or
                "PasswordRequiresUpper" =>
                sharedLocalizer["PasswordInvalidError"].Value,

            _ => sharedLocalizer["UserOperationError"].Value
        };
    }

    private async Task<List<string>> GetAssignableRolesAsync()
    {
        return await roleManager.Roles
            .Where(role =>
                role.Name == EmployeeRole ||
                role.Name == AdminRole)
            .OrderBy(role => role.Name == EmployeeRole ? 0 : 1)
            .Select(role => role.Name!)
            .ToListAsync();
    }

}
