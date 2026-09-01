using HotelManagementSystem.Application.Common.Errors;
using FluentResults;
using HotelManagementSystem.Application.Services;
using HotelManagementSystem.Application.Rooms.Commands;
using HotelManagementSystem.Application.RoomTypes.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using HotelManagementSystem.Web.Extensions;
using HotelManagementSystem.Web.ViewModels.Rooms;
using Mediator;

namespace HotelManagementSystem.Web.Controllers;

[Authorize(Roles = "Employee,Admin")]
[Route("Rooms")]
public sealed class RoomsController(
    ISender sender,
    IRoomService roomService,
    RoomListItemViewModelFactory roomListItemViewModelFactory,
    IStringLocalizer<SharedResource> sharedLocalizer) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        int? floor,
        string? roomType,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;

        var result = await sender.Send(
            new GetRoomsQuery(
                floor,
                roomType,
                minPrice,
                maxPrice,
                Math.Max(1, pageNumber),
                pageSize),
            cancellationToken);

        if (TryAddError(result))
        {
            return View(new RoomListViewModel());
        }

        var pagedRooms = result.Value!;
        var allRooms = await roomService.GetAllAsync(cancellationToken);

        return View(new RoomListViewModel
        {
            Rooms = pagedRooms.Items
                .Select(roomListItemViewModelFactory.Create)
                .ToList(),
            Floor = floor,
            RoomType = roomType,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            PageNumber = pagedRooms.PageNumber,
            PageSize = pagedRooms.PageSize,
            TotalCount = pagedRooms.TotalCount,
            TotalPages = pagedRooms.TotalPages,
            Floors = allRooms
                .Select(room => room.Floor)
                .Distinct()
                .OrderBy(item => item)
                .ToList(),
            RoomTypes = allRooms
                .Select(room => room.Type)
                .Where(type => !string.IsNullOrWhiteSpace(type))
                .Distinct()
                .OrderBy(type => type)
                .ToList()
        });
    }

    [HttpGet("Booking")]
    public async Task<IActionResult> Booking(
        DateOnly? startDate,
        DateOnly? endDate,
        int? guestsCount,
        int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 30;
        var model = new RoomPeriodStatusViewModel
        {
            StartDate = startDate ?? DateOnly.FromDateTime(DateTime.Today),
            EndDate = endDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            GuestsCount = Math.Max(1, guestsCount ?? 1),
            PageNumber = Math.Max(1, pageNumber),
            PageSize = pageSize
        };

        try
        {
            var pagedRooms = await roomService.GetPeriodStatusesAsync(
                model.StartDate,
                model.EndDate,
                guestsCount: model.GuestsCount,
                pageNumber: model.PageNumber,
                pageSize: pageSize,
                cancellationToken: cancellationToken);

            model.Rooms = pagedRooms.Items
                .Where(room => room.CanBook)
                .ToList();
            model.TotalCount = pagedRooms.TotalCount;
            model.TotalPages = pagedRooms.TotalPages;
        }
        catch (PersistenceOperationException exception)
        {
            var message = sharedLocalizer["UnexpectedRoomError"];

            if (exception.ErrorCode == PersistenceErrorCode.InvalidDateRange)
            {
                message = sharedLocalizer["InvalidRoomDateRangeError"];
            }

            ModelState.AddModelError(
                string.Empty,
                message);
            model.Rooms = [];
        }
        catch (ArgumentException)
        {
            ModelState.AddModelError(
                string.Empty,
                sharedLocalizer["UnexpectedRoomError"]);
            model.Rooms = [];
        }

        return View(model);
    }

    [HttpPost("{id}/Delete")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteRoomCommand(id),
            cancellationToken);

        if (TryAddError(result, redirect: true))
        {
            return RedirectToAction(nameof(Index));
        }

        TempData.SetSuccessMessage(
            sharedLocalizer["RoomDeleteSuccess"]);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Create")]
    [Authorize(Roles = "Employee,Admin")]
    public IActionResult Create() => View(new RoomCreateViewModel());

    [HttpPost("Create")]
    [Authorize(Roles = "Employee,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        RoomCreateViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var command = new CreateRoomCommand(
            model.RoomNumber,
            model.Floor,
            model.Type,
            model.Description,
            model.PricePerDay,
            model.Capacity,
            model.RoomCount,
            model.OperationalStatus);
        var result = await sender.Send(command, cancellationToken);

        if (TryAddError(result))
        {
            return View(model);
        }

        TempData.SetSuccessMessage(
            sharedLocalizer["RoomCreateSuccess"]);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id}/Edit")]
    [Authorize(Roles = "Employee,Admin")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var rooms = await roomService.GetAllAsync(cancellationToken);
        var room = rooms.FirstOrDefault(item => item.Id == id);

        if (room is null)
        {
            return NotFound();
        }

        return View(new RoomEditViewModel
        {
            Id = id,
            RoomNumber = room.RoomNumber,
            Floor = room.Floor,
            Type = room.Type,
            Description = room.Description,
            PricePerDay = room.PricePerDay,
            Capacity = room.Capacity,
            RoomCount = room.RoomCount,
            OperationalStatus = room.OperationalStatus
        });
    }

    [HttpPost("{id}/Edit")]
    [Authorize(Roles = "Employee,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        RoomEditViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var command = new UpdateRoomCommand(
            id,
            model.RoomNumber,
            model.Floor,
            model.Type,
            model.Description,
            model.PricePerDay,
            model.Capacity,
            model.RoomCount,
            model.OperationalStatus);
        var result = await sender.Send(command, cancellationToken);

        if (TryAddError(result))
        {
            if (result.GetCode() == "Room.NotFound")
            {
                return NotFound();
            }

            return View(model);
        }

        TempData.SetSuccessMessage(
            sharedLocalizer["RoomUpdateSuccess"]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id}/Status")]
    [Authorize(Roles = "Employee,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(
        int id,
        RoomStatusViewModel model,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ChangeRoomStatusCommand(
                id,
                model.OperationalStatus),
            cancellationToken);

        if (TryAddError(result, redirect: true))
        {
            return RedirectToAction(nameof(Index));
        }

        TempData.SetSuccessMessage(
            sharedLocalizer["RoomStatusUpdateSuccess"]);

        return RedirectToAction(nameof(Index));
    }

    private bool TryAddError<T>(
        Result<T> result,
        bool redirect = false)
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
        else if (result.GetCode() == "Room.DuplicateRoomNumber")
        {
            ModelState.AddModelError(
                nameof(RoomCreateViewModel.RoomNumber),
                message);
        }
        else
        {
            ModelState.AddModelError(
                string.Empty,
                message);
        }

        return true;
    }

    private string GetResultErrorMessage<T>(Result<T> result)
    {
        return result.GetCode() switch
        {
            "Room.DuplicateRoomNumber" =>
                sharedLocalizer["RoomDuplicateNumberError"].Value,
            "Room.NotFound" =>
                sharedLocalizer["RoomNotFoundError"].Value,
            "Room.DeleteFailed" =>
                sharedLocalizer["RoomDeleteError"].Value,
            _ => sharedLocalizer["UnexpectedRoomError"].Value
        };
    }
}

