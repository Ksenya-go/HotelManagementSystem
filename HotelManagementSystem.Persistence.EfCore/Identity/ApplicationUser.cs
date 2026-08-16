using Microsoft.AspNetCore.Identity;

namespace HotelManagementSystem.Persistence.EfCore.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
