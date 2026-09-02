using FluentResults;
using HotelManagementSystem.Application.Common.Errors;
using HotelManagementSystem.Application.RoomTypes.Commands;
using HotelManagementSystem.Application.RoomTypes.Queries;
using HotelManagementSystem.Web.Extensions;
using HotelManagementSystem.Web.ViewModels.Admin;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HotelManagementSystem.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/RoomTypes")]
public sealed class AdminRoomTypesController(
    ISender sender,
    IStringLocalizer<SharedResource> sharedLocalizer) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetRoomTypesQuery(),
            cancellationToken);

        if (TryAddError(result))
        {
            return View(new AdminRoomTypesViewModel
            {
                RoomTypes = []
            });
        }

        return View(new AdminRoomTypesViewModel
        {
            RoomTypes = result.Value!
        });
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new RoomTypeFormViewModel());
    }

    [HttpGet("{id}/Edit")]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetRoomTypesQuery(),
            cancellationToken);

        if (TryAddError(result))
        {
            return NotFound();
        }

        var roomType = result.Value?
            .FirstOrDefault(item => item.Id == id);

        if (roomType is null)
        {
            return NotFound();
        }

        return View("Create", new RoomTypeFormViewModel
        {
            Id = roomType.Id,
            Name = roomType.Name,
            Description = roomType.Description,
            BasePrice = roomType.BasePrice,
            MaxGuests = roomType.MaxGuests
        });
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        RoomTypeFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Id == 0)
        {
            var result = await sender.Send(
                new CreateRoomTypeCommand(
                    model.Name,
                    model.Description,
                    model.BasePrice,
                    model.MaxGuests),
                cancellationToken);

            if (!TryAddError(result))
            {
                TempData.SetSuccessMessage(
                    sharedLocalizer["RoomTypeCreateSuccess"]);

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        var updateResult = await sender.Send(
            new UpdateRoomTypeCommand(
                model.Id,
                model.Name,
                model.Description,
                model.BasePrice,
                model.MaxGuests),
            cancellationToken);

        if (!updateResult.IsSuccess)
        {
            if (updateResult.GetCode() == "RoomType.NotFound")
            {
                return NotFound();
            }

            TryAddError(updateResult);

            return View(model);
        }

        TempData.SetSuccessMessage(
            sharedLocalizer["RoomTypeUpdateSuccess"]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteRoomTypeCommand(id),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData.SetErrorMessage(
                GetResultErrorMessage(result));

            return RedirectToAction(nameof(Index));
        }

        TempData.SetSuccessMessage(
            sharedLocalizer["RoomTypeDeleteSuccess"]);

        return RedirectToAction(nameof(Index));
    }

    private bool TryAddError<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return false;
        }

        var message = GetResultErrorMessage(result);

        ModelState.AddModelError(
            string.Empty,
            message);

        return true;
    }

    private string GetResultErrorMessage<T>(Result<T> result)
    {
        return result.GetCode() switch
        {
            "RoomType.NotFound" =>
                sharedLocalizer["RoomTypeNotFoundError"].Value,

            "RoomType.DeleteFailed" =>
                sharedLocalizer["RoomTypeDeleteError"].Value,

            _ =>
                sharedLocalizer["UnexpectedRoomTypeError"].Value
        };
    }
}