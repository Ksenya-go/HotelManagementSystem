import httpClient from "./httpClient";
import type { ManagerReportDto, ManagerReportQuery } from "@/types/report";

export const reportsApi = {
  get: (query: ManagerReportQuery) =>
    httpClient.get<ManagerReportDto>("/reports", { params: query }).then((r) => r.data),
};