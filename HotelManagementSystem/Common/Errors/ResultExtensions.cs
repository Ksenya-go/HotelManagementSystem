using FluentResults;

namespace HotelManagementSystem.Application.Common.Errors;

public static class ResultExtensions
{
    public static string? GetCode(this ResultBase result) =>
        result.Errors
            .OfType<AppError>()
            .FirstOrDefault()?.Code;
}