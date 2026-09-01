
using HotelManagementSystem.Application.Common.Cqrs.Results;
using Mediator;

namespace HotelManagementSystem.Application.Rooms.Commands;

public sealed record DeleteRoomCommand(int Id) : ICommand<Result<Unit>>;
