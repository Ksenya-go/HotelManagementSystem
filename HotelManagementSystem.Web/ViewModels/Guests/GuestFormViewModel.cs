using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Web.ViewModels.Guests;

public sealed class GuestFormViewModel
{
    [Required(ErrorMessage = "Вкажіть ім’я.")]
    [StringLength(100, ErrorMessage = "Ім’я не може перевищувати 100 символів.")]
    [Display(Name = "Ім’я")]
    public string FirstName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Вкажіть прізвище.")]
    [StringLength(100, ErrorMessage = "Прізвище не може перевищувати 100 символів.")]
    [Display(Name = "Прізвище")]
    public string LastName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Вкажіть електронну пошту.")]
    [EmailAddress(ErrorMessage = "Вкажіть коректну електронну пошту.")]
    [Display(Name = "Електронна пошта")]
    public string Email { get; set; } = string.Empty;
    [StringLength(40, ErrorMessage = "Телефон не може перевищувати 40 символів.")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;
}

