using HotelManagementSystem.Application.Rooms.Commands;
using HotelManagementSystem.Application.RoomTypes;
using HotelManagementSystem.Application.RoomTypes.Queries;
using HotelManagementSystem.Application.Services;
using HotelManagementSystem.Domain.Rooms;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Web.Controllers.Api;

public sealed record RoomListResponse(
    IReadOnlyList<RoomDto> Rooms,
    IReadOnlyList<int> Floors,
    IReadOnlyList<string> RoomTypes,
    int TotalPages,
    int TotalCount);

public sealed record RoomPeriodStatusResponse(
    IReadOnlyList<RoomPeriodStatusDto> Rooms,
    IReadOnlyList<int> Floors,
    IReadOnlyList<string> RoomTypes,
    int TotalPages,
    int TotalCount);

public sealed record RoomFormRequest(
    string RoomNumber,
    int Floor,
    string Type,
    string Description,
    decimal PricePerDay,
    int Capacity,
    int RoomCount,
    RoomOperationalStatus OperationalStatus);

[ApiController]
[Route("api/rooms")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Employee,Admin")]
public sealed class RoomsApiController(ISender sender, IRoomService roomService) : ControllerBase
{
    // GET /api/rooms?floor=&roomType=&minPrice=&maxPrice=&pageNumber=&pageSize=
    [HttpGet]
    public async Task<ActionResult<RoomListResponse>> List(
        int? floor, string? roomType, decimal? minPrice, decimal? maxPrice,
        int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetRoomsQuery(floor, roomType, minPrice, maxPrice,
                Math.Max(1, pageNumber), Math.Clamp(pageSize, 1, 100)), ct);

        if (result.IsFailed)
        {
            return BadRequest(new { message = result.Errors.FirstOrDefault()?.Message ?? "Не вдалося виконати операцію з номером." });
        }

        var paged = result.Value;
        var (floors, roomTypes) = await LoadFiltersAsync(ct);

        return Ok(new RoomListResponse(paged.Items, floors, roomTypes, paged.TotalPages, paged.TotalCount));
    }

    // GET /api/rooms/period-status?startDate=&endDate=&guestsCount=&floor=&roomType=...
    [HttpGet("period-status")]
    public async Task<ActionResult<RoomPeriodStatusResponse>> PeriodStatus(
        DateOnly startDate, DateOnly endDate, int? guestsCount, int? floor, string? roomType,
        decimal? minPrice, decimal? maxPrice, int pageNumber = 1, int pageSize = 30,
        CancellationToken ct = default)
    {
        if (endDate <= startDate)
        {
            return BadRequest(new { message = "Дата закінчення має бути пізнішою за дату початку." });
        }

        var paged = await roomService.GetPeriodStatusesAsync(
            startDate, endDate, floor, roomType, minPrice, maxPrice, guestsCount,
            Math.Max(1, pageNumber), Math.Clamp(pageSize, 1, 100), ct);

        var (floors, roomTypes) = await LoadFiltersAsync(ct);

        return Ok(new RoomPeriodStatusResponse(paged.Items, floors, roomTypes, paged.TotalPages, paged.TotalCount));
    }

    // GET /api/rooms/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomDto>> GetById(int id, CancellationToken ct)
    {
        var rooms = await roomService.GetAllAsync(ct);
        var room = rooms.FirstOrDefault(r => r.Id == id);
        return room is null ? NotFound() : Ok(room);
    }

    // POST /api/rooms
    [HttpPost]
    public async Task<ActionResult<IReadOnlyList<RoomDto>>> Create(
        [FromBody] RoomFormRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new CreateRoomCommand(
                request.RoomNumber, request.Floor, request.Type, request.Description,
                request.PricePerDay, request.Capacity, request.RoomCount, request.OperationalStatus),
            ct);

        if (result.IsFailed)
        {
            return BadRequest(new { message = MapError(result.Errors.FirstOrDefault()?.Message) });
        }

        return Ok(result.Value);
    }

    // PUT /api/rooms/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] RoomFormRequest request, CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateRoomCommand(
                id, request.RoomNumber, request.Floor, request.Type, request.Description,
                request.PricePerDay, request.Capacity, request.RoomCount, request.OperationalStatus),
            ct);

        if (result.IsFailed)
        {
            var message = result.Errors.FirstOrDefault()?.Message;
            if (message == "Room.NotFound") return NotFound();
            return BadRequest(new { message = MapError(message) });
        }

        return NoContent();
    }

    // DELETE /api/rooms/{id}
    [HttpDelete("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteRoomCommand(id), ct);
        if (result.IsFailed)
        {
            return BadRequest(new { message = MapError(result.Errors.FirstOrDefault()?.Message) });
        }

        return NoContent();
    }

    private static string MapError(string? code) => code switch
    {
        "Room.DuplicateRoomNumber" => "Номер з таким номером кімнати вже існує.",
        "Room.NotFound" => "Номер не знайдено.",
        "Room.DeleteFailed" => "Не вдалося видалити номер.",
        _ => "Не вдалося виконати операцію з номером.",
    };

    private async Task<(IReadOnlyList<int> Floors, IReadOnlyList<string> RoomTypes)> LoadFiltersAsync(CancellationToken ct)
    {
        var all = await roomService.GetAllAsync(ct);
        var floors = all.Select(r => r.Floor).Distinct().OrderBy(f => f).ToList();
        var types = all.Select(r => r.Type)
            .Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().OrderBy(t => t).ToList();
        return (floors, types);
    }
}