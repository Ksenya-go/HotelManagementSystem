import httpClient from "./httpClient";
import type {
  ReservationsIndexQuery,
  ReservationsIndexResponse,
  ReservationFormInput,
  ReservationEditInput,
  ReservationDto,
  ReservationStatusInput,
} from "@/types/reservation";

export const reservationsApi = {
  list: (query: ReservationsIndexQuery) =>
    httpClient
      .get<ReservationsIndexResponse>("/reservations", { params: query })
      .then((r) => r.data),

  getById: (id: number) =>
    httpClient.get<ReservationEditInput>(`/reservations/${id}`).then((r) => r.data),

  create: (input: ReservationFormInput) =>
    httpClient.post<ReservationDto>("/reservations", input).then((r) => r.data),

  update: (id: number, input: ReservationEditInput) =>
    httpClient.put<void>(`/reservations/${id}`, input),

  remove: (id: number) => httpClient.delete<void>(`/reservations/${id}`),

  changeStatus: (input: ReservationStatusInput) =>
    httpClient.patch<void>(`/reservations/${input.id}/status`, input),
};