using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Web.ViewModels.Reservation;

public sealed class ReservationFormViewModel
{
    public int? GuestId { get; set; }
    [StringLength(100, ErrorMessage = "Ім’я не може перевищувати 100 символів.")]
    [Display(Name = "Ім’я")]
    public string NewGuestFirstName { get; set; } = string.Empty;
    [StringLength(100, ErrorMessage = "Прізвище не може перевищувати 100 символів.")]
    [Display(Name = "Прізвище")]
    public string NewGuestLastName { get; set; } = string.Empty;
    [EmailAddress(ErrorMessage = "Вкажіть коректну електронну пошту.")]
    [Display(Name = "Електронна пошта")]
    public string NewGuestEmail { get; set; } = string.Empty;
    [StringLength(40, ErrorMessage = "Телефон не може перевищувати 40 символів.")]
    [Display(Name = "Телефон")]
    public string NewGuestPhone { get; set; } = string.Empty;
    [Required(ErrorMessage = "Оберіть кімнату.")]
    [Display(Name = "Кімната")]
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int RoomFloor { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public decimal RoomPricePerDay { get; set; }
    public int RoomCapacity { get; set; }
    [Required(ErrorMessage = "Вкажіть дату заселення.")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата заселення")]
    public DateOnly CheckIn { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string CheckInTime { get; set; } = "14:00";
    [Required(ErrorMessage = "Вкажіть дату виселення.")]
    [DataType(DataType.Date)]
    [Display(Name = "Дата виселення")]
    public DateOnly CheckOut { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
    public string CheckOutTime { get; set; } = "12:00";
    [Range(1, 20, ErrorMessage = "Кількість гостей має бути від 1 до 20.")]
    [Display(Name = "Кількість гостей")]
    public int GuestsCount { get; set; }
}

