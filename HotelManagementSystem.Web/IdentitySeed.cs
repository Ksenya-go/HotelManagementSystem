using HotelManagementSystem.Persistence.EfCore.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Web;

public static class IdentitySeed
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        var seedOptions = new SeedOptions();
        configuration.GetSection("Seed").Bind(seedOptions);

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = seedOptions.Roles.Where(role => !string.IsNullOrWhiteSpace(role)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                EnsureSucceeded(result);
            }
        }

        if (!await dbContext.SystemSettings.AnyAsync() && seedOptions.SystemSettings.Length > 0)
        {
            dbContext.SystemSettings.AddRange(seedOptions.SystemSettings
                .Where(setting => !string.IsNullOrWhiteSpace(setting.Key))
                .Select(setting => new Domain.Entities.SystemSetting(setting.Key, setting.Value, setting.Description)));
            await dbContext.SaveChangesAsync();
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        await NormalizeEmployeeRolesAsync(userManager, roleManager, roles);
        var adminEmail = configuration["SeedAdmin:Email"];
        var adminPassword = configuration["SeedAdmin:Password"];
        var adminName = configuration["SeedAdmin:FullName"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword) || string.IsNullOrWhiteSpace(adminName))
        {
            return;
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = adminName
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            EnsureSucceeded(result);
        }
        else
        {
            if (!await userManager.CheckPasswordAsync(admin, adminPassword))
            {
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(admin);
                var passwordResult = await userManager.ResetPasswordAsync(admin, resetToken, adminPassword);
                EnsureSucceeded(passwordResult);
            }

            if (!string.Equals(admin.FullName, adminName, StringComparison.Ordinal))
            {
                admin.FullName = adminName;
                var updateResult = await userManager.UpdateAsync(admin);
                EnsureSucceeded(updateResult);
            }
        }

        var adminRole = roles.FirstOrDefault(role => string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(adminRole))
        {
            return;
        }

        if (!await userManager.IsInRoleAsync(admin, adminRole))
        {
            var result = await userManager.AddToRoleAsync(admin, adminRole);
            EnsureSucceeded(result);
        }
    }

    private static async Task NormalizeEmployeeRolesAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, string[] roles)
    {
        var employeeRole = roles.FirstOrDefault(role => string.Equals(role, "Employee", StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(employeeRole)) return;
        if (!await roleManager.RoleExistsAsync(employeeRole)) return;

        var oldRoleNames = new[] { "Manager", "Receptionist" };
        foreach (var user in await userManager.Users.ToListAsync())
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            var oldOperationalRoles = currentRoles.Where(role => oldRoleNames.Contains(role)).ToArray();
            if (oldOperationalRoles.Length == 0) continue;

            var removeResult = await userManager.RemoveFromRolesAsync(user, oldOperationalRoles);
            EnsureSucceeded(removeResult);
            if (!await userManager.IsInRoleAsync(user, employeeRole))
            {
                var addResult = await userManager.AddToRoleAsync(user, employeeRole);
                EnsureSucceeded(addResult);
            }
        }

        foreach (var oldRoleName in oldRoleNames)
        {
            var oldRole = await roleManager.FindByNameAsync(oldRoleName);
            if (oldRole is null) continue;

            var deleteResult = await roleManager.DeleteAsync(oldRole);
            EnsureSucceeded(deleteResult);
        }
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }
}
