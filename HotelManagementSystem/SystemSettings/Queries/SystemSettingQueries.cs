using HotelManagementSystem.Application.Common.Cqrs.Abstractions;
using HotelManagementSystem.Application.Common.Cqrs.Results;

namespace HotelManagementSystem.Application.SystemSettings.Queries;

public sealed record GetSystemSettingsQuery : IQuery<Result<IReadOnlyList<SystemSettingDto>>>;
