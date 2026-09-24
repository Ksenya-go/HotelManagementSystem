import { z } from "zod";
import { RoomOperationalStatus } from "@/types/enums";

export const roomSchema = z.object({
  type: z
    .string()
    .min(1, "Вкажіть тип кімнати.")
    .max(100, "Тип кімнати не може перевищувати 100 символів."),
  description: z
    .string()
    .max(500, "Опис не може перевищувати 500 символів.")
    .optional()
    .default(""),
  pricePerDay: z
    .number({ invalid_type_error: "Ціна має бути числом." })
    .min(0, "Ціна має бути від 0 до 999999.")
    .max(999999, "Ціна має бути від 0 до 999999."),
  capacity: z
    .number({ invalid_type_error: "Місткість має бути числом." })
    .min(1, "Місткість номера має бути від 1 до 20.")
    .max(20, "Місткість номера має бути від 1 до 20."),
  roomCount: z
    .number({ invalid_type_error: "Кількість кімнат має бути числом." })
    .min(1, "Кількість кімнат має бути від 1 до 20.")
    .max(20, "Кількість кімнат має бути від 1 до 20."),
  roomNumber: z
    .string()
    .min(1, "Вкажіть номер кімнати.")
    .max(20, "Номер кімнати не може перевищувати 20 символів."),
  floor: z
    .number({ invalid_type_error: "Поверх має бути числом." })
    .min(1, "Поверх має бути від 1 до 9.")
    .max(9, "Поверх має бути від 1 до 9."),
  operationalStatus: z.enum([
    RoomOperationalStatus.Clean,
    RoomOperationalStatus.Cleaning,
    RoomOperationalStatus.InMaintenance,
  ]),
});

export type RoomFormValues = z.infer<typeof roomSchema>;

export const roomDefaultValues: RoomFormValues = {
  type: "",
  description: "",
  pricePerDay: 0,
  capacity: 1,
  roomCount: 1,
  roomNumber: "",
  floor: 1,
  operationalStatus: RoomOperationalStatus.Clean,
};