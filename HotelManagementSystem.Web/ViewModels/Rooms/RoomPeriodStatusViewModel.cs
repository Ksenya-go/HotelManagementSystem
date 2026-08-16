using System.ComponentModel.DataAnnotations;
using HotelManagementSystem.Application.RoomTypes;

namespace HotelManagementSystem.Web.ViewModels.Rooms;

public sealed class RoomPeriodStatusViewModel
{
    [Required(ErrorMessage = "Вкажіть дату початку.")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата початку")]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "Вкажіть дату закінчення.")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата закінчення")]
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

    [Range(1, 20, ErrorMessage = "Кількість гостей має бути від 1 до 20.")]
    [Display(Name = "Кількість гостей")]
    public int? GuestsCount { get; set; }

    public IReadOnlyList<RoomPeriodStatusDto> Rooms { get; set; } = [];

    [Display(Name = "Поверх")]
    public int? Floor { get; set; }

    [Display(Name = "Тип кімнати")]
    public string? RoomType { get; set; }

    [Display(Name = "Ціна від")]
    public decimal? MinPrice { get; set; }

    [Display(Name = "Ціна до")]
    public decimal? MaxPrice { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 30;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public IReadOnlyList<int> Floors { get; set; } = [];
    public IReadOnlyList<string> RoomTypes { get; set; } = [];
}

