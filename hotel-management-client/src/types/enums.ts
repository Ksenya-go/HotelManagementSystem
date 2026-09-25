export const ReservationStatus = {
  Pending: "Pending",
  Confirmed: "Confirmed",
  CheckedIn: "CheckedIn",
  CheckedOut: "CheckedOut",
  Cancelled: "Cancelled",
} as const;

export type ReservationStatus = (typeof ReservationStatus)[keyof typeof ReservationStatus];

export const RoomOperationalStatus = {
  Clean: "Clean",
  Cleaning: "Cleaning",
  InMaintenance: "InMaintenance",
} as const;

export type RoomOperationalStatus =
  (typeof RoomOperationalStatus)[keyof typeof RoomOperationalStatus];

export const RoomAvailabilityStatus = {
  Available: "Available",
  Occupied: "Occupied",
} as const;

export type RoomAvailabilityStatus =
  (typeof RoomAvailabilityStatus)[keyof typeof RoomAvailabilityStatus];