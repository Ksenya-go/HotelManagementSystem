using HotelManagementSystem.Application.Common.Cqrs.Abstractions;
using HotelManagementSystem.Application.Common.Cqrs.Results;

namespace HotelManagementSystem.Application.Rooms.Commands;

public sealed record DeleteRoomCommand(int Id) : ICommand<Result<Unit>>;
