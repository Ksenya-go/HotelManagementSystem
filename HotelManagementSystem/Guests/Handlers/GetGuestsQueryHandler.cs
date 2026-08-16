using HotelManagementSystem.Application.Common.Cqrs.Abstractions;
using HotelManagementSystem.Application.Common.Cqrs.Results;
using HotelManagementSystem.Application.Guests.Queries;
using HotelManagementSystem.Application.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace HotelManagementSystem.Application.Guests.Handlers;

public sealed class GetGuestsQueryHandler(
    IGuestService guestService)
    : IQueryHandler<GetGuestsQuery, Result<IReadOnlyList<GuestDto>>>
{
    public async ValueTask<Result<IReadOnlyList<GuestDto>>> Handle(
        GetGuestsQuery request,
        CancellationToken cancellationToken)
    {
        var guests = await guestService.GetAllAsync(
            request.Search,
            cancellationToken);

        return Result<IReadOnlyList<GuestDto>>.Ok(guests);
    }
}
