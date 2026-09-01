using HotelManagementSystem.Application.Common.Cqrs.Results;
using HotelManagementSystem.Application.Common.Pagination;
using Mediator;

namespace HotelManagementSystem.Application.RoomTypes.Queries;

public sealed record GetRoomsQuery(int? Floor = null, string? RoomType = null, decimal? 
    MinPrice = null, decimal? MaxPrice = null, int PageNumber = 1, int PageSize = 30) 
    : IQuery<Result<PagedResult<RoomDto>>>;

