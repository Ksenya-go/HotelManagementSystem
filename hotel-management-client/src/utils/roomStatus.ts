import { RoomOperationalStatus } from "@/types/enums";

export const roomStatusLabels: Record<RoomOperationalStatus, string> = {
  [RoomOperationalStatus.Clean]: "Доступний",
  [RoomOperationalStatus.Cleaning]: "Потребує прибирання",
  [RoomOperationalStatus.InMaintenance]: "На обслуговуванні",
};