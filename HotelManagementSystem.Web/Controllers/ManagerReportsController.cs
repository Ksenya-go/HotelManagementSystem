using HotelManagementSystem.Application.Services;
using HotelManagementSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Web.Controllers;

[Authorize(Roles = "Employee,Admin")]
[Route("Reports")]
public sealed class ManagerReportsController(
    IManagerReportingService reportingService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        ManagerReportViewModel model,
        CancellationToken cancellationToken)
    {
        if (model.To <= model.From)
        {
            model.To = model.From.AddDays(1);
        }

        model.Report = await reportingService.GetReportAsync(
            model.From,
            model.To,
            cancellationToken);

        return View(model);
    }
}