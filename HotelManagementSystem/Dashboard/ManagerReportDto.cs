namespace HotelManagementSystem.Application.Dashboard;

public sealed record ManagerReportDto(DateOnly From, 
    DateOnly To, int Reservations, int ActiveReservations, 
    int CompletedReservations, int CancelledReservations, int TotalGuests, 
    int OccupiedRoomDays, int AvailableRoomDays, decimal OccupancyRate, 
    decimal CancellationRate, decimal AverageStayDays);

