using FluentResults;
using Mediator;

namespace HotelManagementSystem.Application.RoomTypes.Queries;

public sealed record GetRoomTypesQuery : IQuery<Result<IReadOnlyList<RoomTypeDto>>>;
