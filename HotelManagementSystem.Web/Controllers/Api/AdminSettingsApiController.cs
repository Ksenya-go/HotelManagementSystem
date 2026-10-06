using HotelManagementSystem.Application.SystemSettings;
using HotelManagementSystem.Application.SystemSettings.Commands;
using HotelManagementSystem.Application.SystemSettings.Queries;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Web.Controllers.Api;

public sealed record SystemSettingResponse(int Id, string Key, string DisplayName, string Value);

public sealed record UpdateSettingRequest(string Value);

[ApiController]
[Route("api/admin/settings")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public sealed class AdminSettingsApiController(ISender sender) : ControllerBase
{
    // GET /api/admin/settings
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SystemSettingResponse>>> List(CancellationToken ct)
    {
        var result = await sender.Send(new GetSystemSettingsQuery(), ct);

        if (result.IsFailed)
        {
            return BadRequest(new { message = "Не вдалося виконати операцію з налаштуваннями." });
        }

        return Ok(result.Value.Select(Map).ToList());
    }

    // PUT /api/admin/settings/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSettingRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Value))
        {
            return BadRequest(new { message = "Вкажіть значення налаштування." });
        }

        var result = await sender.Send(new UpdateSystemSettingCommand(id, request.Value), ct);

        if (result.IsFailed)
        {
            var message = result.Errors.FirstOrDefault()?.Message;
            if (message == "SystemSetting.NotFound") return NotFound();
            return BadRequest(new { message = "Не вдалося виконати операцію з налаштуваннями." });
        }

        return NoContent();
    }

    private static SystemSettingResponse Map(SystemSettingDto setting)
    {
        var displayName = setting.Key switch
        {
            "hotel.checkInTime" => "Час заселення",
            "hotel.checkOutTime" => "Час виселення",
            "hotel.currency" => "Валюта",
            "hotel.name" => "Назва готелю",
            _ => setting.Description,
        };

        return new SystemSettingResponse(setting.Id, setting.Key, displayName, setting.Value);
    }
}