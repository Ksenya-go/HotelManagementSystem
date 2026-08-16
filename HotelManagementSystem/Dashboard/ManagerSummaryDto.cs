namespace HotelManagementSystem.Application.Dashboard;

public sealed record ManagerSummaryDto(int TotalRooms, int AvailableRooms, int OccupiedRooms, 
    int CleaningRooms, int MaintenanceRooms, int TodaysReservations, int PendingReservations, 
    decimal OccupancyRate);

