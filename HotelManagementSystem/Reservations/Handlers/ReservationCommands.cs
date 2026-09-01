using HotelManagementSystem.Application.Common.Cqrs.Results;
using HotelManagementSystem.Application.Common.Errors;
using HotelManagementSystem.Application.Reservations.Commands;
using HotelManagementSystem.Application.Services;
using Mediator;

namespace HotelManagementSystem.Application.Reservations.Handlers;

public sealed class ReservationCommandHandler(IReservationService service)
    : ICommandHandler<CreateReservationCommand, Result<ReservationDto>>,
      ICommandHandler<UpdateReservationCommand, Result<Unit>>,
      ICommandHandler<ChangeReservationStatusCommand, Result<Unit>>,
      ICommandHandler<CancelReservationCommand, Result<Unit>>,
      ICommandHandler<DeleteReservationCommand, Result<Unit>>,
      ICommandHandler<CheckInReservationCommand, Result<Unit>>,
      ICommandHandler<CheckOutReservationCommand, Result<Unit>>
{
    public async ValueTask<Result<ReservationDto>> Handle(CreateReservationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var reservation = await service.CreateAsync(request, cancellationToken);

            return Result<ReservationDto>.Ok(reservation);
        }
        catch (PersistenceOperationException exception)
        {
            return ReservationErrors.Invalid(
                PersistenceErrorMessages.GetMessage(exception));
        }
        catch (Exception exception)
            when (exception is ArgumentException)
        {
            return ReservationErrors.Invalid(exception.Message);
        }
    }

    public async ValueTask<Result<Unit>> Handle(UpdateReservationCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var updated = await service.UpdateAsync(request, cancellationToken);

            return updated
                ? Result<Unit>.Ok(Unit.Value)
                : ReservationErrors.NotFound();
        }
        catch (PersistenceOperationException exception)
        {
            return ReservationErrors.InvalidUnit(
                PersistenceErrorMessages.GetMessage(exception));
        }
    }

    public async ValueTask<Result<Unit>> Handle(ChangeReservationStatusCommand request,
        CancellationToken cancellationToken)
    {
        var updated = await service.ChangeStatusAsync(request, cancellationToken);

        return updated
            ? Result<Unit>.Ok(Unit.Value)
            : ReservationErrors.NotFound();
    }

    public async ValueTask<Result<Unit>> Handle(
        CancelReservationCommand request,
        CancellationToken cancellationToken)
    {
        var cancelled = await service.CancelAsync(
            request.Id,
            cancellationToken);

        return cancelled
            ? Result<Unit>.Ok(Unit.Value)
            : ReservationErrors.NotFound();
    }

    public async ValueTask<Result<Unit>> Handle(DeleteReservationCommand request,
        CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(
            request.Id,
            cancellationToken);

        return deleted
            ? Result<Unit>.Ok(Unit.Value)
            : ReservationErrors.NotFound();
    }

    public async ValueTask<Result<Unit>> Handle(CheckInReservationCommand request,
        CancellationToken cancellationToken)
    {
        var checkedIn = await service.CheckInAsync(
            request.Id,
            cancellationToken);

        return checkedIn
            ? Result<Unit>.Ok(Unit.Value)
            : ReservationErrors.CheckInFailed();
    }

    public async ValueTask<Result<Unit>> Handle(
        CheckOutReservationCommand request,
        CancellationToken cancellationToken)
    {
        var checkedOut = await service.CheckOutAsync(
            request.Id,
            cancellationToken);

        return checkedOut
            ? Result<Unit>.Ok(Unit.Value)
            : ReservationErrors.CheckOutFailed();
    }
}