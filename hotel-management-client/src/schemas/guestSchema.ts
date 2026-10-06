import { z } from "zod";

export const guestSchema = z.object({
  firstName: z
    .string()
    .min(1, "Вкажіть ім'я.")
    .max(100, "Ім'я не може перевищувати 100 символів."),
  lastName: z
    .string()
    .min(1, "Вкажіть прізвище.")
    .max(100, "Прізвище не може перевищувати 100 символів."),
  email: z
    .string()
    .min(1, "Вкажіть електронну пошту.")
    .email("Вкажіть коректну електронну пошту."),
  phone: z
    .string()
    .max(40, "Телефон не може перевищувати 40 символів.")
    .optional()
    .default(""),
});

export type GuestFormValues = z.infer<typeof guestSchema>;