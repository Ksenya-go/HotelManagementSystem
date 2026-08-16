using HotelManagementSystem.Application.Common.Cqrs.Results;

namespace HotelManagementSystem.Application.SystemSettings;

public static class SystemSettingErrors
{
    public static Result<Unit> NotFound() =>
        Result<Unit>.Fail(
            "SystemSetting.NotFound",
            "Налаштування не знайдено.");
}
