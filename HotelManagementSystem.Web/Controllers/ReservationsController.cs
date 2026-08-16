using HotelManagementSystem.Application.Reservations.Queries;
using HotelManagementSystem.Application.SystemSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CreateReservationCqrs = HotelManagementSystem.Application.Reservations.Commands.CreateReservationCommand;
using UpdateReservationCqrs = HotelManagementSystem.Application.Reservations.Commands.UpdateReservationCommand;
using HotelManagementSystem.Domain.Reservation;
using HotelManagementSystem.Application.Common.Cqrs.Abstractions;
using HotelManagementSystem.Application.Reservations.Commands;
using HotelManagementSystem.Application.Services;
using HotelManagementSystem.Application.Common.Cqrs.Results;
using HotelManagementSystem.Web.Extensions;
using HotelManagementSystem.Web.ViewModels.Reservation;
using Microsoft.Extensions.Localization;

namespace HotelManagementSystem.Web.Controllers;

[Authorize(Roles = "Employee,Admin")]
[Route("Reservations")]
public sealed class ReservationsController(
    ISender sender,
    IGuestService guestService,
    IRoomService roomService,
    ISystemSettingService settingService,
    IStringLocalizer<SharedResource> sharedLocalizer) : Controller
{
    private const string CheckInTimeKey = "hotel.checkInTime";
    private const string CheckOutTimeKey = "hotel.checkOutTime";

    [HttpGet("")]
    public async Task<IActionResult> Index(
        ReservationStatus? status,
        string? guestSearch,
        string? roomNumber,
        int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;
        var reservationTimes = await GetReservationTimesAsync(cancellationToken);
        var queryResult = await sender.Send(
            new GetReservationsQuery(
                status,
                GuestSearch: guestSearch,
                RoomNumber: roomNumber,
                PageNumber: Math.Max(1, pageNumber),
                PageSize: pageSize),
            cancellationToken);
        var result = queryResult.Value;
        return View(new ReservationsIndexViewModel
        {
            Reservations = result?.Items ?? [],
            Status = status,
            CheckInTime = reservationTimes.CheckInTime,
            CheckOutTime = reservationTimes.CheckOutTime,
            GuestSearch = guestSearch,
            RoomNumber = roomNumber,
            PageNumber = result?.PageNumber ?? Math.Max(1, pageNumber),
            PageSize = result?.PageSize ?? pageSize,
            TotalCount = result?.TotalCount ?? 0,
            TotalPages = result?.TotalPages ?? 0
        });
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(
        int? roomId,
        DateOnly? checkIn,
        DateOnly? checkOut,
        int guestsCount = 1,
        CancellationToken cancellationToken = default)
    {
        if (!roomId.HasValue || !checkIn.HasValue || !checkOut.HasValue)
        {
            return RedirectToAction("Booking", "ManagerRooms", new
            {
                startDate = checkIn,
                endDate = checkOut,
                guestsCount = Math.Max(1, guestsCount)
            });
        }

        guestsCount = Math.Max(1, guestsCount);
        var roomStatus = (await roomService.GetPeriodStatusesAsync(
            checkIn.Value,
            checkOut.Value,
            guestsCount: guestsCount,
            pageNumber: 1,
            pageSize: int.MaxValue,
            cancellationToken: cancellationToken)).Items.FirstOrDefault(room => room.Id == roomId.Value);
        if (roomStatus is null || !roomStatus.CanBook)
        {
            TempData.SetErrorMessage(
                sharedLocalizer["ReservationRoomUnavailable"]);

            return RedirectToAction(
                "Booking",
                "ManagerRooms",
                new
                {
                    startDate = checkIn.Value,
                    endDate = checkOut.Value,
                    guestsCount
                });
        }

        var model = new ReservationFormViewModel
        {
            RoomId = roomId.Value,
            CheckIn = checkIn.Value,
            CheckOut = checkOut.Value,
            GuestsCount = guestsCount
        };

        await LoadSelectedRoomAsync(model, cancellationToken);
        await LoadReservationTimesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ReservationFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectedRoomAsync(model, cancellationToken);
            await LoadReservationTimesAsync(model, cancellationToken);
            return View(model);
        }

        var selectedRooms = await roomService.GetAllAsync(cancellationToken);
        var selectedRoom = selectedRooms.FirstOrDefault(
            room => room.Id == model.RoomId);

        if (selectedRoom is null)
        {
            return NotFound();
        }

        if (model.GuestsCount > selectedRoom.Capacity)
        {
            ModelState.AddModelError(
                nameof(model.GuestsCount),
                sharedLocalizer[
                    "ReservationRoomCapacityExceeded",
                    selectedRoom.Capacity]);
        }

        if (!ModelState.IsValid)
        {
            await LoadSelectedRoomAsync(model, cancellationToken);
            await LoadReservationTimesAsync(model, cancellationToken);
            return View(model);
        }

        int? guestId;

        try
        {
            guestId = await ResolveGuestIdAsync(
                model.GuestId,
                model.NewGuestFirstName,
                model.NewGuestLastName,
                model.NewGuestEmail,
                model.NewGuestPhone,
                cancellationToken);
        }
        catch (ArgumentException)
        {
            ModelState.AddModelError(
                string.Empty,
                sharedLocalizer["GuestDataInvalid"]);
            await LoadSelectedRoomAsync(model, cancellationToken);
            await LoadReservationTimesAsync(model, cancellationToken);
            return View(model);
        }

        if (guestId is null)
        {
            ModelState.AddModelError(
                string.Empty,
                sharedLocalizer["GuestDataRequired"]);
            await LoadSelectedRoomAsync(model, cancellationToken);
            await LoadReservationTimesAsync(model, cancellationToken);
            return View(model);
        }

        var result = await sender.Send(
            new CreateReservationCqrs(
                guestId.Value,
                model.RoomId,
                model.CheckIn,
                model.CheckOut,
                model.GuestsCount),
            cancellationToken);

        if (TryAddError(result))
        {
            await LoadSelectedRoomAsync(model, cancellationToken);
            await LoadReservationTimesAsync(model, cancellationToken);
            return View(model);
        }

        TempData.SetSuccessMessage(
            sharedLocalizer["ReservationCreateSuccess"]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id}/CheckIn")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CheckInReservationCommand(id),
            cancellationToken);

        if (TryAddError(result, redirect: true))
        {
            return RedirectToAction(nameof(Index));
        }

        TempData.SetSuccessMessage(
            sharedLocalizer["ReservationCheckInSuccess"]);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id}/CheckOut")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CheckOutReservationCommand(id),
            cancellationToken);
        if (TryAddError(result, redirect: true))
        {
            return RedirectToAction(nameof(Index));
        }

        TempData.SetSuccessMessage(
            sharedLocalizer["ReservationCheckOutSuccess"]);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id}/Status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(
        int id,
        ReservationStatusViewModel model,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ChangeReservationStatusCommand(id, model.Status),
            cancellationToken);

        if (TryAddError(result, redirect: true))
        {
            return RedirectToAction(nameof(Index));
        }

        TempData.SetSuccessMessage(
            sharedLocalizer["ReservationStatusUpdateSuccess"]);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id}/Edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetReservationsQuery(PageSize: int.MaxValue),
            cancellationToken);
        var reservation = result.Value?.Items.FirstOrDefault(
            item => item.Id == id);

        if (reservation is null)
        {
            return NotFound();
        }
        var nameParts = reservation.GuestName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var reservationTimes = await GetReservationTimesAsync(cancellationToken);
        return View(new ReservationEditViewModel
        {
            Id = id,
            GuestId = reservation.GuestId,
            NewGuestFirstName = nameParts.ElementAtOrDefault(0) ?? string.Empty,
            NewGuestLastName = nameParts.ElementAtOrDefault(1) ?? string.Empty,
            NewGuestEmail = reservation.GuestEmail,
            NewGuestPhone = reservation.GuestPhone,
            RoomNumber = reservation.RoomNumber,
            RoomFloor = reservation.RoomFloor,
            RoomType = reservation.RoomType,
            RoomPricePerDay = reservation.RoomPricePerDay,
            RoomCapacity = reservation.RoomCapacity,
            CheckIn = reservation.CheckIn,
            CheckOut = reservation.CheckOut,
            CheckInTime = reservationTimes.CheckInTime,
            CheckOutTime = reservationTimes.CheckOutTime,
            GuestsCount = reservation.GuestsCount
        });
    }

    [HttpPost("{id}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        ReservationEditViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadReservationTimesAsync(model, cancellationToken);
            return View(model);
        }
        int? guestId;

        try
        {
            guestId = await ResolveGuestIdAsync(
                model.GuestId,
                model.NewGuestFirstName,
                model.NewGuestLastName,
                model.NewGuestEmail,
                model.NewGuestPhone,
                cancellationToken);
        }
        catch (ArgumentException)
        {
            ModelState.AddModelError(
                string.Empty,
                sharedLocalizer["GuestDataInvalid"]);
            await LoadReservationTimesAsync(model, cancellationToken);
            return View(model);
        }

        if (guestId is null)
        {
            ModelState.AddModelError(
                string.Empty,
                sharedLocalizer["GuestDataRequired"]);
            await LoadReservationTimesAsync(model, cancellationToken);
            return View(model);
        }

        var result = await sender.Send(
            new UpdateReservationCqrs(
                id,
                guestId.Value,
                model.CheckIn,
                model.CheckOut,
                model.GuestsCount),
            cancellationToken);

        if (TryAddError(result))
        {
            await LoadReservationTimesAsync(model, cancellationToken);
            return View(model);
        }

        TempData.SetSuccessMessage(
            sharedLocalizer["ReservationUpdateSuccess"]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id}/Cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CancelReservationCommand(id),
            cancellationToken);
        if (TryAddError(result, redirect: true))
        {
            return RedirectToAction(nameof(Index));
        }

        TempData.SetSuccessMessage(
            sharedLocalizer["ReservationCancelSuccess"]);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id}/Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteReservationCommand(id),
            cancellationToken);
        if (TryAddError(result, redirect: true))
        {
            return RedirectToAction(nameof(Index));
        }

        TempData.SetSuccessMessage(
            sharedLocalizer["ReservationDeleteSuccess"]);
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSelectedRoomAsync(
        ReservationFormViewModel model,
        CancellationToken cancellationToken)
    {
        var rooms = await roomService.GetAllAsync(cancellationToken);
        var room = rooms.FirstOrDefault(item => item.Id == model.RoomId);

        if (room is null)
        {
            return;
        }

        model.RoomNumber = room.RoomNumber;
        model.RoomFloor = room.Floor;
        model.RoomType = room.Type;
        model.RoomPricePerDay = room.PricePerDay;
        model.RoomCapacity = room.Capacity;
    }

    private async Task LoadReservationTimesAsync(
        ReservationFormViewModel model,
        CancellationToken cancellationToken)
    {
        var reservationTimes = await GetReservationTimesAsync(cancellationToken);
        model.CheckInTime = reservationTimes.CheckInTime;
        model.CheckOutTime = reservationTimes.CheckOutTime;
    }

    private async Task LoadReservationTimesAsync(
        ReservationEditViewModel model,
        CancellationToken cancellationToken)
    {
        var reservationTimes = await GetReservationTimesAsync(cancellationToken);
        model.CheckInTime = reservationTimes.CheckInTime;
        model.CheckOutTime = reservationTimes.CheckOutTime;
    }

    private async Task<(string CheckInTime, string CheckOutTime)> GetReservationTimesAsync(
        CancellationToken cancellationToken)
    {
        var settings = await settingService.GetAllAsync(cancellationToken);
        var checkInTime = settings.FirstOrDefault(setting => setting.Key == CheckInTimeKey)?.Value;
        var checkOutTime = settings.FirstOrDefault(setting => setting.Key == CheckOutTimeKey)?.Value;

        var resolvedCheckInTime = string.IsNullOrWhiteSpace(checkInTime)
            ? "14:00"
            : checkInTime;
        var resolvedCheckOutTime = string.IsNullOrWhiteSpace(checkOutTime)
            ? "12:00"
            : checkOutTime;

        return (resolvedCheckInTime, resolvedCheckOutTime);
    }

    private async Task<int?> ResolveGuestIdAsync(
        int? selectedGuestId,
        string firstName,
        string lastName,
        string email,
        string phone,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        if (selectedGuestId.GetValueOrDefault() > 0)
        {
            var updateCommand = new HotelManagementSystem.Application.Guests.Commands.UpdateGuestCommand(
                selectedGuestId.Value,
                firstName,
                lastName,
                email,
                phone);

            await guestService.UpdateAsync(updateCommand, cancellationToken);
            return selectedGuestId.Value;
        }

        var createCommand = new HotelManagementSystem.Application.Guests.Commands.CreateGuestCommand(
            firstName,
            lastName,
            email,
            phone);
        var guest = await guestService.CreateAsync(
            createCommand,
            cancellationToken);

        return guest.Id;
    }

    private bool TryAddError<T>(Result<T> result, bool redirect = false)
    {
        if (result.IsSuccess)
        {
            return false;
        }

        var message = GetResultErrorMessage(result);

        if (redirect)
        {
            TempData.SetErrorMessage(message);
        }
        else
        {
            ModelState.AddModelError(string.Empty, message);
        }

        return true;
    }

    private string GetResultErrorMessage<T>(Result<T> result)
    {
        return result.Error?.Code switch
        {
            "Reservation.NotFound" =>
                sharedLocalizer["ReservationNotFoundError"].Value,
            "Reservation.CheckInFailed" =>
                sharedLocalizer["ReservationCheckInError"].Value,
            "Reservation.CheckOutFailed" =>
                sharedLocalizer["ReservationCheckOutError"].Value,
            "Reservation.Invalid" =>
                sharedLocalizer["ReservationInvalidError"].Value,
            _ => sharedLocalizer["UnexpectedReservationError"].Value
        };
    }
}

