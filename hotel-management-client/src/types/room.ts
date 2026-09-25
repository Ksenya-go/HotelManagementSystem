import type { RoomAvailabilityStatus, RoomOperationalStatus } from "./enums";

// Відповідає RoomDto
export interface RoomListItem {
  id: number;
  roomNumber: string;
  floor: number;
  type: string;
  description: string;
  pricePerDay: number;
  capacity: number;
  roomCount: number;
  bookedDates: string[];
  operationalStatus: RoomOperationalStatus;
  availabilityStatus: RoomAvailabilityStatus;
}

// Відповідає RoomListViewModel (фільтри + пагінація)
export interface RoomListQuery {
  floor?: number;
  roomType?: string;
  minPrice?: number;
  maxPrice?: number;
  pageNumber: number;
  pageSize: number;
}

export interface RoomListResponse {
  rooms: RoomListItem[];
  floors: number[];
  roomTypes: string[];
  totalPages: number;
  totalCount: number;
}

// Відповідає RoomCreateViewModel / CreateRoomCommand
export interface RoomCreateInput {
  roomNumber: string;
  floor: number;
  type: string;
  description: string;
  pricePerDay: number;
  capacity: number;
  roomCount: number;
  operationalStatus: RoomOperationalStatus;
}

// Відповідає RoomEditViewModel / UpdateRoomCommand
export interface RoomEditInput extends RoomCreateInput {
  id: number;
}

// Відповідає RoomStatusViewModel
export interface RoomStatusInput {
  id: number;
  operationalStatus: RoomOperationalStatus;
}

// Відповідає RoomPeriodStatusDto
export interface RoomPeriodStatusDto {
  id: number;
  roomNumber: string;
  floor: number;
  type: string;
  description: string;
  pricePerDay: number;
  capacity: number;
  roomCount: number;
  operationalStatus: string;
  availabilityStatus: string;
  canBook: boolean;
  isAvailable: boolean;
}

// Відповідає RoomPeriodStatusViewModel (запит)
export interface RoomPeriodStatusQuery {
  startDate: string;
  endDate: string;
  guestsCount?: number;
  floor?: number;
  roomType?: string;
  minPrice?: number;
  maxPrice?: number;
  pageNumber: number;
  pageSize: number;
}

export interface RoomPeriodStatusResponse {
  rooms: RoomPeriodStatusDto[];
  floors: number[];
  roomTypes: string[];
  totalPages: number;
  totalCount: number;
}