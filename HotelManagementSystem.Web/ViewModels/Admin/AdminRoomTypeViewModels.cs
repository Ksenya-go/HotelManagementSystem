using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Web.ViewModels.Admin;

public sealed class RoomTypeFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Вкажіть назву типу номера.")]
    [StringLength(100, ErrorMessage = "Назва типу номера не може перевищувати 100 символів.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Опис не може перевищувати 500 символів.")]
    public string Description { get; set; } = string.Empty;

    [Range(0, 999999, ErrorMessage = "Базова ціна має бути від 0 до 999999.")]
    public decimal BasePrice { get; set; }

    [Range(1, 20, ErrorMessage = "Кількість гостей має бути від 1 до 20.")]
    public int MaxGuests { get; set; } = 1;
}
