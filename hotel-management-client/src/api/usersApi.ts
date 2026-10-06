import httpClient from "./httpClient";
import type {
  CreateUserInput,
  UpdateUserInput,
  UserDetailsDto,
  UserRowDto,
} from "@/types/user";

export const usersApi = {
  // GET Admin/Users
  list: () => httpClient.get<UserRowDto[]>("/admin/users").then((r) => r.data),

  // GET Admin/Users/{id}/Edit
  getById: (id: string) =>
    httpClient.get<UserDetailsDto>(`/admin/users/${id}`).then((r) => r.data),

  // POST Admin/Users/Create
  create: (input: CreateUserInput) => httpClient.post<void>("/admin/users", input),

  // POST Admin/Users/{id}/Edit
  update: (id: string, input: UpdateUserInput) =>
    httpClient.put<void>(`/admin/users/${id}`, input),

  // POST Admin/Users/{id}/ToggleLockout
  toggleLockout: (id: string) => httpClient.post<void>(`/admin/users/${id}/toggle-lockout`),

  // POST Admin/Users/{id}/ChangeRole
  changeRole: (id: string, role: string) =>
    httpClient.post<void>(`/admin/users/${id}/change-role`, { role }),
};