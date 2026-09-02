using HotelManagementSystem.Application.SystemSettings;
using HotelManagementSystem.Application.SystemSettings.Commands;
using FluentResults;
using HotelManagementSystem.Application.Common.Errors;
using HotelManagementSystem.Application.SystemSettings.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.Web.ViewModels.Admin;
using HotelManagementSystem.Web.Extensions;
using Microsoft.Extensions.Localization;
using Mediator;

namespace HotelManagementSystem.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("Admin/Settings")]
public sealed class AdminSettingsController(
    ISender sender,
    SystemSettingItemViewModelFactory systemSettingItemViewModelFactory,
    IStringLocalizer<SharedResource> sharedLocalizer) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetSystemSettingsQuery(),
            cancellationToken);

        if (TryAddError(result))
        {
            return View(new AdminSettingsViewModel());
        }

        return View(CreateViewModel(result.Value!));
    }

    [HttpPost("Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        SystemSettingFormViewModel model,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Value))
        {
            ModelState.AddModelError(
                nameof(model.Value),
                sharedLocalizer["SettingValueRequired"]);
        }

        if (!ModelState.IsValid)
        {
            var settingsResult = await sender.Send(
                new GetSystemSettingsQuery(),
                cancellationToken);

            return View(
                "Index",
                settingsResult.IsSuccess
                    ? CreateViewModel(settingsResult.Value!)
                    : new AdminSettingsViewModel());
        }

        var result = await sender.Send(
            new UpdateSystemSettingCommand(
                model.Id,
                model.Value),
            cancellationToken);

        if (result.IsSuccess)
        {
            TempData.SetSuccessMessage(
                sharedLocalizer["SettingUpdateSuccess"]);
        }
        else
        {
            TempData.SetErrorMessage(
                GetResultErrorMessage(result));
        }

        return RedirectToAction(nameof(Index));
    }

    private AdminSettingsViewModel CreateViewModel(
        IReadOnlyList<SystemSettingDto> settings)
    {
        return new AdminSettingsViewModel
        {
            Settings = settings
                .Select(systemSettingItemViewModelFactory.Create)
                .ToList()
        };
    }

    private bool TryAddError<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return false;
        }

        ModelState.AddModelError(
            string.Empty,
            GetResultErrorMessage(result));

        return true;
    }

    private string GetResultErrorMessage<T>(Result<T> result)
    {
        return result.GetCode() switch
        {
            "SystemSetting.NotFound" =>
                sharedLocalizer["SettingNotFoundError"].Value,

            _ =>
                sharedLocalizer["UnexpectedSettingError"].Value
        };
    }
}