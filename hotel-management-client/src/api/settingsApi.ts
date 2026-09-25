import httpClient from "./httpClient";
import type { SystemSettingDto } from "@/types/settings";

export const settingsApi = {
  list: () => httpClient.get<SystemSettingDto[]>("/admin/settings").then((r) => r.data),

  update: (id: number, value: string) =>
    httpClient.put<void>(`/admin/settings/${id}`, { value }),
};