import { ReservationStatus } from "@/types/enums";

export const reservationStatusLabels: Record<ReservationStatus, string> = {
  [ReservationStatus.Pending]: "Очікує підтвердження",
  [ReservationStatus.Confirmed]: "Підтверджено",
  [ReservationStatus.CheckedIn]: "Заселено",
  [ReservationStatus.CheckedOut]: "Виселено",
  [ReservationStatus.Cancelled]: "Скасовано",
};

export const reservationStatuses: ReservationStatus[] = [
  ReservationStatus.Pending,
  ReservationStatus.Confirmed,
  ReservationStatus.CheckedIn,
  ReservationStatus.CheckedOut,
  ReservationStatus.Cancelled,
];