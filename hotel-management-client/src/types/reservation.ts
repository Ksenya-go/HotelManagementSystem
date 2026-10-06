import type { ReservationStatus } from "./enums";

export interface ReservationFormInput {
  guestId?: number | null;
  newGuestFirstName: string;
  newGuestLastName: string;
  newGuestEmail: string;
  newGuestPhone: string;
  roomId: number;
  checkIn: string; // yyyy-MM-dd
  checkOut: string;
  guestsCount: number;
}

export interface ReservationEditInput extends ReservationFormInput {
  id: number;
}

export interface ReservationStatusInput {
  id: number;
  status: ReservationStatus;
}

export interface ReservationDto {
  id: number;
  guestId: number;
  guestFullName: string;
  guestEmail: string;
  guestPhone: string;
  roomNumber: string;
  roomFloor: number;
  roomType: string;
  roomPricePerDay: number;
  roomCapacity: number;
  checkIn: string;
  checkOut: string;
  guestsCount: number;
  status: ReservationStatus;
  totalPrice: number;
}

export interface ReservationsIndexQuery {
  status?: ReservationStatus;
  checkInFrom?: string;
  checkInTo?: string;
  checkOutFrom?: string;
  checkOutTo?: string;
  guestSearch?: string;
  roomNumber?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface ReservationsIndexResponse {
  reservations: ReservationDto[];
  checkInTime: string;
  checkOutTime: string;
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}