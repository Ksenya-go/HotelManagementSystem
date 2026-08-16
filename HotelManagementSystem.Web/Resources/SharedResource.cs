namespace HotelManagementSystem.Web;

public sealed class SharedResource
{
    public static string FullName => nameof(FullName);
    public static string FullNameRequired => nameof(FullNameRequired);
    public static string FullNameLength => nameof(FullNameLength);
    public static string Email => nameof(Email);
    public static string EmailRequired => nameof(EmailRequired);
    public static string EmailInvalid => nameof(EmailInvalid);
    public static string Role => nameof(Role);
    public static string RoleRequired => nameof(RoleRequired);
    public static string Password => nameof(Password);
    public static string PasswordRequired => nameof(PasswordRequired);
    public static string PasswordLength => nameof(PasswordLength);
    public static string NewPassword => nameof(NewPassword);
}

