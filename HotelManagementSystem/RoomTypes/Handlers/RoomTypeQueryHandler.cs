
using HotelManagementSystem.Application.Common.Cqrs.Results;
using HotelManagementSystem.Application.RoomTypes.Queries;
using Mediator;

namespace HotelManagementSystem.Application.RoomTypes.Handlers;

public sealed class RoomTypeQueryHandler(IRoomTypeService service)
    : IQueryHandler<GetRoomTypesQuery, Result<IReadOnlyList<RoomTypeDto>>>
{
    public async ValueTask<Result<IReadOnlyList<RoomTypeDto>>> Handle(
        GetRoomTypesQuery request,
        CancellationToken cancellationToken)
    {
        var roomTypes = await service.GetAllAsync(cancellationToken);

        return Result<IReadOnlyList<RoomTypeDto>>.Ok(roomTypes);
    }
}