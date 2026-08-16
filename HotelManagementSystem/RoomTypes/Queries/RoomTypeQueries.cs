using HotelManagementSystem.Application.Common.Cqrs.Abstractions;
using HotelManagementSystem.Application.Common.Cqrs.Results;

namespace HotelManagementSystem.Application.RoomTypes.Queries;

public sealed record GetRoomTypesQuery : IQuery<Result<IReadOnlyList<RoomTypeDto>>>;
