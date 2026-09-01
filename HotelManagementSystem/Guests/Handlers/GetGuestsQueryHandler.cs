using HotelManagementSystem.Application.Common.Cqrs.Results;
using HotelManagementSystem.Application.Guests.Queries;
using HotelManagementSystem.Application.Services;
using Mediator;
using System;
using System.Collections.Generic;


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
