import { z } from "zod";

export const loginSchema = z.object({
  email: z
    .string()
    .trim()
    .min(1, "Вкажіть електронну пошту.")
    .email("Вкажіть коректну електронну пошту."),
  password: z.string().min(1, "Вкажіть пароль."),
  rememberMe: z.boolean(),
});

export type LoginFormValues = z.infer<typeof loginSchema>;