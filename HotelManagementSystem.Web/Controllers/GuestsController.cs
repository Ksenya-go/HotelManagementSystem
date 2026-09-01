using HotelManagementSystem.Application.Guests.Queries;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Web.Controllers;

[Authorize(Roles = "Employee,Admin")]
[Route("Guests")]
public sealed class GuestsController(ISender sender) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetGuestsQuery(query),
            cancellationToken);

        return View(result.Value);
    }
}