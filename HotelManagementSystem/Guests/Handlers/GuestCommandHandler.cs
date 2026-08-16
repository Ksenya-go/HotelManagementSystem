using HotelManagementSystem.Application.Common.Cqrs.Abstractions;
using HotelManagementSystem.Application.Common.Cqrs.Results;
using HotelManagementSystem.Application.Guests.Commands;
using HotelManagementSystem.Application.Services;

namespace HotelManagementSystem.Application.Guests.Handlers;

public sealed class GuestCommandHandler(IGuestService service)
    : ICommandHandler<CreateGuestCommand, Result<GuestDto>>,
      ICommandHandler<UpdateGuestCommand, Result<Unit>>
{
    public async ValueTask<Result<GuestDto>> Handle(
        CreateGuestCommand request,
        CancellationToken cancellationToken)
    {
        var guest = await service.CreateAsync(request, cancellationToken);

        return Result<GuestDto>.Ok(guest);
    }

    public async ValueTask<Result<Unit>> Handle(
        UpdateGuestCommand request,
        CancellationToken cancellationToken)
    {
        var updated = await service.UpdateAsync(request, cancellationToken);

        return updated
            ? Result<Unit>.Ok(Unit.Value)
            : GuestErrors.NotFound();
    }
}