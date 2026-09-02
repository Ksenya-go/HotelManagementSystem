using HotelManagementSystem.Domain.Reservations;

namespace HotelManagementSystem.Domain.Guests;

public sealed class Guest
{
    private Guest()
    {
    }

    public Guest(
        string firstName,
        string lastName,
        string email,
        string phone)
    {
        Update(firstName, lastName, email, phone);
    }

    public int Id { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string Phone { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public ICollection<Reservation> Reservations { get; private set; } =
        new List<Reservation>();

    public void Update(
        string firstName,
        string lastName,
        string email,
        string phone)
    {
        Validate(firstName, lastName, email);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim();
        Phone = phone?.Trim() ?? string.Empty;
    }

    private static void Validate(string firstName, string lastName, string email)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException(
                GuestValidationMessages.FirstNameRequired,
                nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException(
                GuestValidationMessages.LastNameRequired,
                nameof(lastName));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException(
                GuestValidationMessages.EmailRequired,
                nameof(email));
        }
    }
}