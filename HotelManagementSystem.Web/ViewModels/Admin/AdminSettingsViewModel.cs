namespace HotelManagementSystem.Web.ViewModels.Admin;

public sealed record SystemSettingItemViewModel(int Id, string Key, string DisplayName, string Value);

public sealed class SystemSettingFormViewModel
{
    public int Id { get; set; }

    public string Value { get; set; } = string.Empty;
}

public sealed class AdminSettingsViewModel
{
    public IReadOnlyList<SystemSettingItemViewModel> Settings { get; init; } = [];
}