using FluentResults;

namespace HotelManagementSystem.Application.Common.Errors;

public sealed class AppError : Error
{
    public string Code { get; }

    public AppError(string code, string message) : base(message)
    {
        Code = code;
    }
}