import httpClient from "./httpClient";
import type {
  RoomListQuery,
  RoomListResponse,
  RoomCreateInput,
  RoomEditInput,
  RoomListItem,
  RoomStatusInput,
  RoomPeriodStatusQuery,
  RoomPeriodStatusResponse,
} from "@/types/room";


export const roomsApi = {
  list: (query: RoomListQuery) =>
    httpClient.get<RoomListResponse>("/rooms", { params: query }).then((r) => r.data),

  getById: (id: number) =>
    httpClient.get<RoomEditInput>(`/rooms/${id}`).then((r) => r.data),

  create: (input: RoomCreateInput) =>
    httpClient.post<RoomListItem>("/rooms", input).then((r) => r.data),

  update: (id: number, input: RoomEditInput) =>
    httpClient.put<void>(`/rooms/${id}`, input),

  remove: (id: number) => httpClient.delete<void>(`/rooms/${id}`),

  changeStatus: (input: RoomStatusInput) =>
    httpClient.patch<void>(`/rooms/${input.id}/status`, input),

  periodStatus: (query: RoomPeriodStatusQuery) =>
    httpClient
      .get<RoomPeriodStatusResponse>("/rooms/period-status", { params: query })
      .then((r) => r.data),
};