import { z } from "zod";

export const loginSchema = z.object({
  email: z
    .string()
    .min(1, "Вкажіть електронну пошту.")
    .email("Вкажіть коректну електронну пошту."),
  password: z.string().min(1, "Вкажіть пароль."),
  rememberMe: z.boolean().default(false),
});

export type LoginFormValues = z.infer<typeof loginSchema>;