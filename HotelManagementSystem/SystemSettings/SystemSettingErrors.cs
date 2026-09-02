using FluentResults;
using HotelManagementSystem.Application.Common.Errors;
using Mediator;

namespace HotelManagementSystem.Application.SystemSettings;

public static class SystemSettingErrors
{
    public static Result<Unit> NotFound() =>
        Result.Fail<Unit>(
            new AppError(
                "SystemSetting.NotFound",
                "Налаштування не знайдено."));
}
