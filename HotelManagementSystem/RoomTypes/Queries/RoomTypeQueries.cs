using HotelManagementSystem.Application.Common.Cqrs.Results;
using Mediator;

namespace HotelManagementSystem.Application.RoomTypes.Queries;

public sealed record GetRoomTypesQuery : IQuery<Result<IReadOnlyList<RoomTypeDto>>>;
