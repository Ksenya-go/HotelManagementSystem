using HotelManagementSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Web.Controllers;

[AllowAnonymous]
public sealed class ErrorController : Controller
{
    [Route("Error")]
    public IActionResult Index() => View(new ErrorViewModel
    {
        RequestId = HttpContext.TraceIdentifier
    });
}
