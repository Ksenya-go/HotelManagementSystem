import { z } from "zod";

export const reservationSchema = z.object({
  guestId: z.number().nullable().optional(),

  newGuestFirstName: z
    .string()
    .trim()
    .max(100, "Ім'я не може перевищувати 100 символів."),
  newGuestLastName: z
    .string()
    .trim()
    .max(100, "Прізвище не може перевищувати 100 символів."),
  newGuestEmail: z
    .string()
    .trim()
    .email("Вкажіть коректну електронну пошту.")
    .or(z.literal("")),
  newGuestPhone: z
    .string()
    .trim()
    .max(30, "Телефон не може перевищувати 30 символів."),

  roomId: z
    .number({ error: "Вкажіть кімнату." })
    .min(1, "Вкажіть коректний номер кімнати."),

  checkIn: z.string().min(1, "Вкажіть дату заселення."),
  checkInTime: z.string().min(1, "Вкажіть час заселення."),
  checkOut: z.string().min(1, "Вкажіть дату виселення."),
  checkOutTime: z.string().min(1, "Вкажіть час виселення."),

  guestsCount: z
    .number({ error: "Вкажіть кількість гостей." })
    .min(1, "Кількість гостей має бути від 1 до 20.")
    .max(20, "Кількість гостей має бути від 1 до 20."),
});

export type ReservationFormValues = z.infer<typeof reservationSchema>;