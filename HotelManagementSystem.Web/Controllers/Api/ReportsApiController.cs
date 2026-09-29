using HotelManagementSystem.Application.Dashboard;
using HotelManagementSystem.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Web.Controllers.Api;

[ApiController]
[Route("api/reports")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Employee,Admin")]
public sealed class ReportsApiController(IManagerReportingService reportingService) : ControllerBase
{
    // GET /api/reports?from=&to=
    [HttpGet]
    public async Task<ActionResult<ManagerReportDto>> Get(
        DateOnly from, DateOnly to, CancellationToken ct)
    {
        if (to <= from)
        {
            to = from.AddDays(1);
        }

        var report = await reportingService.GetReportAsync(from, to, ct);
        return Ok(report);
    }
}