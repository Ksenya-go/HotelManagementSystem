import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useState } from "react";
import { reservationsApi } from "@/api/reservationsApi";
import { reservationSchema, type ReservationFormValues } from "@/schemas/reservationSchema";

export default function ReservationCreate() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ReservationFormValues>({
    resolver: zodResolver(reservationSchema),
    defaultValues: {
      guestId: null,
      newGuestFirstName: "",
      newGuestLastName: "",
      newGuestEmail: "",
      newGuestPhone: "",
      roomId: searchParams.get("roomId") ? Number(searchParams.get("roomId")) : 0,
      checkIn: searchParams.get("checkIn") ?? "",
      checkInTime: "14:00",
      checkOut: searchParams.get("checkOut") ?? "",
      checkOutTime: "12:00",
      guestsCount: 1,
    },
  });

  const onSubmit = async (values: ReservationFormValues) => {
    setServerError(null);
    try {
      await reservationsApi.create(values as any);
      navigate("/reservations");
    } catch {
      setServerError("Не вдалося створити бронювання. Перевірте дані.");
    }
  };

  return (
    <div>
      <h1>Нове бронювання</h1>
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        {serverError && <div className="alert alert-danger">{serverError}</div>}

        <fieldset>
          <legend>Гість</legend>
          <p className="text-muted">
            Якщо гість вже є в системі — вкажіть його Id, інакше заповніть поля нового гостя.
          </p>
          <div className="form-group">
            <label htmlFor="guestId">Id існуючого гостя (необов'язково)</label>
            <input
              id="guestId"
              type="number"
              {...register("guestId", { valueAsNumber: true })}
            />
          </div>
          <div className="form-group">
            <label htmlFor="newGuestFirstName">Ім'я</label>
            <input id="newGuestFirstName" {...register("newGuestFirstName")} />
            {errors.newGuestFirstName && (
              <span className="field-error">{errors.newGuestFirstName.message}</span>
            )}
          </div>
          <div className="form-group">
            <label htmlFor="newGuestLastName">Прізвище</label>
            <input id="newGuestLastName" {...register("newGuestLastName")} />
          </div>
          <div className="form-group">
            <label htmlFor="newGuestEmail">Електронна пошта</label>
            <input id="newGuestEmail" type="email" {...register("newGuestEmail")} />
            {errors.newGuestEmail && (
              <span className="field-error">{errors.newGuestEmail.message}</span>
            )}
          </div>
          <div className="form-group">
            <label htmlFor="newGuestPhone">Телефон</label>
            <input id="newGuestPhone" {...register("newGuestPhone")} />
          </div>
        </fieldset>

        <fieldset>
          <legend>Кімната та період</legend>
          <div className="form-group">
            <label htmlFor="roomId">Id кімнати</label>
            <input id="roomId" type="number" {...register("roomId", { valueAsNumber: true })} />
            {errors.roomId && <span className="field-error">{errors.roomId.message}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="checkIn">Дата заселення</label>
            <input id="checkIn" type="date" {...register("checkIn")} />
            {errors.checkIn && <span className="field-error">{errors.checkIn.message}</span>}
          </div>
          <div className="form-group">
            <label htmlFor="checkInTime">Час заселення</label>
            <input id="checkInTime" type="time" {...register("checkInTime")} />
          </div>

          <div className="form-group">
            <label htmlFor="checkOut">Дата виселення</label>
            <input id="checkOut" type="date" {...register("checkOut")} />
            {errors.checkOut && <span className="field-error">{errors.checkOut.message}</span>}
          </div>
          <div className="form-group">
            <label htmlFor="checkOutTime">Час виселення</label>
            <input id="checkOutTime" type="time" {...register("checkOutTime")} />
          </div>

          <div className="form-group">
            <label htmlFor="guestsCount">Кількість гостей</label>
            <input
              id="guestsCount"
              type="number"
              {...register("guestsCount", { valueAsNumber: true })}
            />
            {errors.guestsCount && (
              <span className="field-error">{errors.guestsCount.message}</span>
            )}
          </div>
        </fieldset>

        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Збереження..." : "Забронювати"}
        </button>
      </form>
    </div>
  );
}