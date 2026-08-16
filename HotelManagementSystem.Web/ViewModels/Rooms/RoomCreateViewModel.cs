using System.ComponentModel.DataAnnotations;
using HotelManagementSystem.Domain.Room;

namespace HotelManagementSystem.Web.ViewModels.Rooms;

public sealed class RoomCreateViewModel
{
    [Required(ErrorMessage = "Вкажіть тип кімнати.")]
    [StringLength(100, ErrorMessage = "Тип кімнати не може перевищувати 100 символів.")]
    [Display(Name = "Тип кімнати")]
    public string Type { get; set; } = string.Empty;
    [StringLength(500, ErrorMessage = "Опис не може перевищувати 500 символів.")]
    [Display(Name = "Опис")]
    public string Description { get; set; } = string.Empty;
    [Range(0, 999999, ErrorMessage = "Ціна має бути від 0 до 999999.")]
    [Display(Name = "Ціна за добу")]
    public decimal PricePerDay { get; set; }
    [Range(1, 20, ErrorMessage = "Місткість номера має бути від 1 до 20.")]
    [Display(Name = "Місткість номера")]
    public int Capacity { get; set; } = 1;
    [Range(1, 20, ErrorMessage = "Кількість кімнат має бути від 1 до 20.")]
    [Display(Name = "Кількість кімнат")]
    public int RoomCount { get; set; } = 1;
    [Required(ErrorMessage = "Вкажіть номер кімнати.")]
    [StringLength(20, ErrorMessage = "Номер кімнати не може перевищувати 20 символів.")]
    [Display(Name = "Номер кімнати")]
    public string RoomNumber { get; set; } = string.Empty;
    [Range(1, 9, ErrorMessage = "Поверх має бути від 1 до 9.")]
    [Display(Name = "Поверх")]
    public int Floor { get; set; } = 1;
    public RoomOperationalStatus OperationalStatus { get; set; } = RoomOperationalStatus.Clean;
}

