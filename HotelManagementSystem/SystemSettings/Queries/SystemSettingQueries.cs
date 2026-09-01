
using FluentResults;
using Mediator;

namespace HotelManagementSystem.Application.SystemSettings.Queries;

public sealed record GetSystemSettingsQuery : IQuery<Result<IReadOnlyList<SystemSettingDto>>>;
