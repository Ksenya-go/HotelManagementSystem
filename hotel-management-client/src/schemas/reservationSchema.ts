import { z } from "zod";

export const reservationSchema = z
  .object({
    guestId: z.number().optional().nullable(),
    newGuestFirstName: z
      .string()
      .max(100, "Ім'я не може перевищувати 100 символів.")
      .optional()
      .default(""),
    newGuestLastName: z
      .string()
      .max(100, "Прізвище не може перевищувати 100 символів.")
      .optional()
      .default(""),
    newGuestEmail: z
      .string()
      .email("Вкажіть коректну електронну пошту.")
      .optional()
      .or(z.literal(""))
      .default(""),
    newGuestPhone: z
      .string()
      .max(40, "Телефон не може перевищувати 40 символів.")
      .optional()
      .default(""),
    roomId: z.number({ required_error: "Оберіть кімнату." }).min(1, "Оберіть кімнату."),
    checkIn: z.string().min(1, "Вкажіть дату заселення."),
    checkInTime: z.string().default("14:00"),
    checkOut: z.string().min(1, "Вкажіть дату виселення."),
    checkOutTime: z.string().default("12:00"),
    guestsCount: z
      .number({ invalid_type_error: "Кількість гостей має бути числом." })
      .min(1, "Кількість гостей має бути від 1 до 20.")
      .max(20, "Кількість гостей має бути від 1 до 20."),
  })
  
  .refine((data) => data.checkIn < data.checkOut, {
    message: "Дата виселення має бути пізніше дати заселення.",
    path: ["checkOut"],
  })

  .refine(
    (data) =>
      !!data.guestId || (!!data.newGuestFirstName && !!data.newGuestLastName && !!data.newGuestEmail),
    {
      message: "Оберіть гостя або заповніть дані нового гостя (ім'я, прізвище, email).",
      path: ["newGuestFirstName"],
    }
  );

export type ReservationFormValues = z.infer<typeof reservationSchema>;