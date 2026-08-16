using HotelManagementSystem.Application.Common.Cqrs.Abstractions;
using HotelManagementSystem.Application.Common.Cqrs.Results;
using HotelManagementSystem.Application.Common.Pagination;
using HotelManagementSystem.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotelManagementSystem.Application.RoomTypes.Queries;

public sealed class GetRoomsQueryHandler(
    IRoomService roomService)
    : IQueryHandler<GetRoomsQuery, Result<PagedResult<RoomDto>>>
{
    public async ValueTask<Result<PagedResult<RoomDto>>> Handle(
        GetRoomsQuery request,
        CancellationToken cancellationToken)
    {
        var rooms = await roomService.GetPagedAsync(
            request.Floor,
            request.RoomType,
            request.MinPrice,
            request.MaxPrice,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result<PagedResult<RoomDto>>.Ok(rooms);
    }
}
