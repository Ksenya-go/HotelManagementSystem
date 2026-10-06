import { z } from "zod";
import { RoomOperationalStatus } from "@/types/enums";

export const roomSchema = z.object({
  roomNumber: z
    .string()
    .trim()
    .min(1, "Вкажіть номер кімнати.")
    .max(20, "Номер кімнати не може перевищувати 20 символів."),

  floor: z
    .number({ error: "Вкажіть поверх." })
    .min(1, "Поверх має бути від 1 до 9.")
    .max(9, "Поверх має бути від 1 до 9."),

  type: z
    .string()
    .trim()
    .min(1, "Вкажіть тип кімнати.")
    .max(100, "Тип кімнати не може перевищувати 100 символів."),

  description: z
    .string()
    .trim()
    .max(500, "Опис не може перевищувати 500 символів."),

  pricePerDay: z
    .number({ error: "Вкажіть ціну за добу." })
    .min(0, "Ціна має бути від 0 до 999999.")
    .max(999999, "Ціна має бути від 0 до 999999."),

  capacity: z
    .number({ error: "Вкажіть місткість номера." })
    .min(1, "Місткість номера має бути від 1 до 20.")
    .max(20, "Місткість номера має бути від 1 до 20."),

  roomCount: z
    .number({ error: "Вкажіть кількість кімнат." })
    .min(1, "Кількість кімнат має бути від 1 до 20.")
    .max(20, "Кількість кімнат має бути від 1 до 20."),

  operationalStatus: z.enum(RoomOperationalStatus),
});

export type RoomFormValues = z.infer<typeof roomSchema>;

export const roomDefaultValues: RoomFormValues = {
  roomNumber: "",
  floor: 1,
  type: "",
  description: "",
  pricePerDay: 0,
  capacity: 1,
  roomCount: 1,
  operationalStatus: RoomOperationalStatus.Clean,
};