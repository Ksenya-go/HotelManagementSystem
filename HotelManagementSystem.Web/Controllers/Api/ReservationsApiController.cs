using HotelManagementSystem.Application.Common.Errors;
using HotelManagementSystem.Application.Guests.Commands;
using HotelManagementSystem.Application.Reservations;
using HotelManagementSystem.Application.Reservations.Commands;
using HotelManagementSystem.Application.Reservations.Queries;
using HotelManagementSystem.Application.Services;
using HotelManagementSystem.Application.SystemSettings;
using HotelManagementSystem.Domain.Reservations;
using Mediator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Web.Controllers.Api;

public sealed record ReservationResponse(
    int Id, int GuestId, string GuestFullName, string GuestEmail, string GuestPhone,
    string RoomNumber, int RoomFloor, string RoomType, decimal RoomPricePerDay, int RoomCapacity,
    DateOnly CheckIn, DateOnly CheckOut, int GuestsCount, decimal TotalPrice, ReservationStatus Status);

public sealed record ReservationsIndexResponse(
    IReadOnlyList<ReservationResponse> Reservations,
    string CheckInTime,
    string CheckOutTime,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record ReservationFormRequest(
    int? GuestId,
    string NewGuestFirstName,
    string NewGuestLastName,
    string NewGuestEmail,
    string NewGuestPhone,
    int RoomId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int GuestsCount);

public sealed record ReservationStatusRequest(ReservationStatus Status);

[ApiController]
[Route("api/reservations")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Employee,Admin")]
public sealed class ReservationsApiController(
    ISender sender,
    IGuestService guestService,
    IRoomService roomService,
    ISystemSettingService settingService) : ControllerBase
{
    private const string CheckInTimeKey = "hotel.checkInTime";
    private const string CheckOutTimeKey = "hotel.checkOutTime";

    // GET /api/reservations?status=&guestSearch=&roomNumber=&checkInFrom=&checkInTo=&pageNumber=&pageSize=
    [HttpGet]
    public async Task<ActionResult<ReservationsIndexResponse>> List(
        ReservationStatus? status, string? guestSearch, string? roomNumber,
        DateOnly? checkInFrom, DateOnly? checkInTo,
        DateOnly? checkOutFrom, DateOnly? checkOutTo,
        int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetReservationsQuery(
                Status: status, CheckInFrom: checkInFrom, CheckInTo: checkInTo,
                CheckOutFrom: checkOutFrom, CheckOutTo: checkOutTo,
                GuestSearch: guestSearch, RoomNumber: roomNumber,
                PageNumber: Math.Max(1, pageNumber), PageSize: Math.Clamp(pageSize, 1, 100)), ct);

        if (result.IsFailed)
        {
            return BadRequest(new { message = "Не вдалося виконати операцію з бронюванням." });
        }

        var paged = result.Value;
        var (checkInTime, checkOutTime) = await GetReservationTimesAsync(ct);

        return Ok(new ReservationsIndexResponse(
            paged.Items.Select(Map).ToList(), checkInTime, checkOutTime,
            paged.PageNumber, paged.PageSize, paged.TotalCount, paged.TotalPages));
    }

    // GET /api/reservations/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservationResponse>> GetById(int id, CancellationToken ct)
    {
        var result = await sender.Send(new GetReservationsQuery(PageSize: int.MaxValue), ct);
        var reservation = result.Value?.Items.FirstOrDefault(r => r.Id == id);
        return reservation is null ? NotFound() : Ok(Map(reservation));
    }

    // POST /api/reservations
    [HttpPost]
    public async Task<ActionResult<ReservationResponse>> Create(
        [FromBody] ReservationFormRequest request, CancellationToken ct)
    {
        var rooms = await roomService.GetAllAsync(ct);
        var room = rooms.FirstOrDefault(r => r.Id == request.RoomId);
        if (room is null)
        {
            return NotFound(new { message = "Номер не знайдено." });
        }

        if (request.GuestsCount > room.Capacity)
        {
            return BadRequest(new { message = $"Кількість гостей перевищує місткість номера ({room.Capacity})." });
        }

        int? guestId;
        try
        {
            guestId = await ResolveGuestIdAsync(request, ct);
        }
        catch (ArgumentException)
        {
            return BadRequest(new { message = "Дані гостя некоректні." });
        }
        catch (PersistenceOperationException exception)
        {
            var message = exception.ErrorCode == PersistenceErrorCode.DuplicateGuestEmail
                ? "Гість з такою електронною поштою вже існує."
                : "Дані гостя некоректні.";
            return BadRequest(new { message });
        }

        if (guestId is null)
        {
            return BadRequest(new { message = "Вкажіть дані гостя." });
        }

        var result = await sender.Send(
            new CreateReservationCommand(guestId.Value, request.RoomId, request.CheckIn, request.CheckOut, request.GuestsCount),
            ct);

        if (result.IsFailed)
        {
            return BadRequest(new { message = "Не вдалося створити бронювання." });
        }

        var created = await sender.Send(new GetReservationsQuery(PageSize: int.MaxValue), ct);
        var dto = created.Value?.Items.FirstOrDefault(r => r.Id == result.Value.Id);
        return dto is null ? Ok() : Ok(Map(dto));
    }

    // PUT /api/reservations/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] ReservationFormRequest request, CancellationToken ct)
    {
        int? guestId;
        try
        {
            guestId = await ResolveGuestIdAsync(request, ct);
        }
        catch (ArgumentException)
        {
            return BadRequest(new { message = "Дані гостя некоректні." });
        }

        if (guestId is null)
        {
            return BadRequest(new { message = "Вкажіть дані гостя." });
        }

        var result = await sender.Send(
            new UpdateReservationCommand(id, guestId.Value, request.CheckIn, request.CheckOut, request.GuestsCount),
            ct);

        if (result.IsFailed)
        {
            return BadRequest(new { message = "Не вдалося оновити бронювання." });
        }

        return NoContent();
    }

    // PATCH /api/reservations/{id}/status
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(
        int id, [FromBody] ReservationStatusRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ChangeReservationStatusCommand(id, request.Status), ct);
        if (result.IsFailed)
        {
            return BadRequest(new { message = "Не вдалося оновити статус бронювання." });
        }

        return NoContent();
    }

    // DELETE /api/reservations/{id}
    [HttpDelete("{id:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteReservationCommand(id), ct);
        if (result.IsFailed)
        {
            return BadRequest(new { message = "Не вдалося видалити бронювання." });
        }

        return NoContent();
    }

    private async Task<int?> ResolveGuestIdAsync(ReservationFormRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.NewGuestFirstName) ||
            string.IsNullOrWhiteSpace(request.NewGuestLastName) ||
            string.IsNullOrWhiteSpace(request.NewGuestEmail))
        {
            return null;
        }

        if (request.GuestId.GetValueOrDefault() > 0)
        {
            await guestService.UpdateAsync(
                new UpdateGuestCommand(request.GuestId!.Value, request.NewGuestFirstName,
                    request.NewGuestLastName, request.NewGuestEmail, request.NewGuestPhone), ct);
            return request.GuestId.Value;
        }

        var guest = await guestService.CreateAsync(
            new CreateGuestCommand(request.NewGuestFirstName, request.NewGuestLastName,
                request.NewGuestEmail, request.NewGuestPhone), ct);
        return guest.Id;
    }

    private async Task<(string CheckInTime, string CheckOutTime)> GetReservationTimesAsync(CancellationToken ct)
    {
        var settings = await settingService.GetAllAsync(ct);
        var checkIn = settings.FirstOrDefault(s => s.Key == CheckInTimeKey)?.Value;
        var checkOut = settings.FirstOrDefault(s => s.Key == CheckOutTimeKey)?.Value;
        return (
            string.IsNullOrWhiteSpace(checkIn) ? "14:00" : checkIn,
            string.IsNullOrWhiteSpace(checkOut) ? "12:00" : checkOut);
    }

    private static ReservationResponse Map(ReservationDto r)
    {
        var nights = Math.Max(1, r.CheckOut.DayNumber - r.CheckIn.DayNumber);
        return new ReservationResponse(
            r.Id, r.GuestId, r.GuestName, r.GuestEmail, r.GuestPhone,
            r.RoomNumber, r.RoomFloor, r.RoomType, r.RoomPricePerDay, r.RoomCapacity,
            r.CheckIn, r.CheckOut, r.GuestsCount, nights * r.RoomPricePerDay, r.Status);
    }
}