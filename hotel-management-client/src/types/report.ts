export interface ManagerReportDto {
  from: string;
  to: string;
  reservations: number;
  activeReservations: number;
  completedReservations: number;
  cancelledReservations: number;
  totalGuests: number;
  occupiedRoomDays: number;
  availableRoomDays: number;
  occupancyRate: number;
  cancellationRate: number;
  averageStayDays: number;
}

export interface ManagerReportQuery {
  from: string;
  to: string;
}