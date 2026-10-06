import { z } from "zod";

const PASSWORD_LENGTH = "Пароль має містити від 8 до 100 символів.";

const fullName = z
  .string()
  .trim()
  .min(1, "Вкажіть ПІБ.")
  .max(120, "ПІБ не може перевищувати 120 символів.");

const email = z
  .string()
  .trim()
  .min(1, "Вкажіть електронну пошту.")
  .email("Вкажіть коректну електронну пошту.");

const role = z.string().min(1, "Оберіть роль.");

export const createUserSchema = z.object({
  fullName,
  email,
  password: z
    .string()
    .min(1, "Вкажіть пароль.")
    .min(8, PASSWORD_LENGTH)
    .max(100, PASSWORD_LENGTH),
  role,
});

export const editUserSchema = z.object({
  fullName,
  email,
  role,
  newPassword: z
    .string()
    .refine((v) => v === "" || (v.length >= 8 && v.length <= 100), PASSWORD_LENGTH),
});

export type CreateUserValues = z.infer<typeof createUserSchema>;
export type EditUserValues = z.infer<typeof editUserSchema>;