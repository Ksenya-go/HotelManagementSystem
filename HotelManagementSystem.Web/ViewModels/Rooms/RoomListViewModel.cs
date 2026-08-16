using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Web.ViewModels.Rooms;

public sealed class RoomListViewModel
{
    public IReadOnlyList<RoomListItemViewModel> Rooms { get; set; } = [];

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

