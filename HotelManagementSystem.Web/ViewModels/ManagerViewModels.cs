using System.ComponentModel.DataAnnotations;
using HotelManagementSystem.Application.Dashboard;

namespace HotelManagementSystem.Web.ViewModels;

public sealed class ManagerReportViewModel
{
    [DataType(DataType.Date)] public DateOnly From { get; set; } = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    [DataType(DataType.Date)] public DateOnly To { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
    public ManagerReportDto? Report { get; set; }
}

