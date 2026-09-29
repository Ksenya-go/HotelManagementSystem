import { z } from "zod";

export const reservationSchema = z.object({
  guestId: z.number().nullable().optional(),

  newGuestFirstName: z
    .string()
    .trim()
    .min(1, "Вкажіть ім'я.")
    .max(100, "Ім'я не може перевищувати 100 символів."),
  newGuestLastName: z
    .string()
    .trim()
    .min(1, "Вкажіть прізвище.")
    .max(100, "Прізвище не може перевищувати 100 символів."),
  newGuestEmail: z
    .string()
    .trim()
    .min(1, "Вкажіть електронну пошту.")
    .email("Вкажіть коректну електронну пошту."),
  newGuestPhone: z
    .string()
    .trim()
    .max(40, "Телефон не може перевищувати 40 символів."),

  roomId: z.number({ error: "Оберіть кімнату." }).min(1, "Оберіть кімнату."),
  checkIn: z.string().min(1, "Вкажіть дату заселення."),
  checkOut: z.string().min(1, "Вкажіть дату виселення."),

  guestsCount: z
    .number({ error: "Вкажіть кількість гостей." })
    .min(1, "Кількість гостей має бути від 1 до 20.")
    .max(20, "Кількість гостей має бути від 1 до 20."),
});

export type ReservationFormValues = z.infer<typeof reservationSchema>;