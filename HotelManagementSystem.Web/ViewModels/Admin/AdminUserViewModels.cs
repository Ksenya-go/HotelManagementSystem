using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Web.ViewModels.Admin;

public sealed record UserRowViewModel(string Id, string Email, string FullName, 
    string Role, bool IsLocked);

public sealed class EditEmployeeViewModel
{
    public IReadOnlyList<string> AvailableRoles { get; set; } = [];

    public string Id { get; set; } = string.Empty;
    [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "FullNameRequired")]
    [StringLength(120, ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "FullNameLength")]
    [Display(Name = "FullName", ResourceType = typeof(SharedResource))]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "EmailRequired")]
    [EmailAddress(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "EmailInvalid")]
    [Display(Name = "Email", ResourceType = typeof(SharedResource))]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "RoleRequired")]
    [Display(Name = "Role", ResourceType = typeof(SharedResource))]
    public string Role { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 8, ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "PasswordLength")]
    [DataType(DataType.Password)]
    [Display(Name = "NewPassword", ResourceType = typeof(SharedResource))]
    public string? NewPassword { get; set; }
}

public sealed class AdminUsersViewModel
{
    public IReadOnlyList<UserRowViewModel> Users { get; init; } = [];
}

public sealed class CreateEmployeeViewModel
{
    public IReadOnlyList<string> AvailableRoles { get; set; } = [];

    [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "FullNameRequired")]
    [StringLength(120, ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "FullNameLength")]
    [Display(Name = "FullName", ResourceType = typeof(SharedResource))]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "EmailRequired")]
    [EmailAddress(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "EmailInvalid")]
    [Display(Name = "Email", ResourceType = typeof(SharedResource))]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "PasswordRequired")]
    [StringLength(100, MinimumLength = 8, ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "PasswordLength")]
    [DataType(DataType.Password)]
    [Display(Name = "Password", ResourceType = typeof(SharedResource))]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessageResourceType = typeof(SharedResource), ErrorMessageResourceName = "RoleRequired")]
    [Display(Name = "Role", ResourceType = typeof(SharedResource))]
    public string Role { get; set; } = string.Empty;
}

