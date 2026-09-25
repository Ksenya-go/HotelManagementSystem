import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate, useParams } from "react-router-dom";
import { reservationsApi } from "@/api/reservationsApi";
import { reservationSchema, type ReservationFormValues } from "@/schemas/reservationSchema";

export default function ReservationEdit() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [serverError, setServerError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ReservationFormValues>({ resolver: zodResolver(reservationSchema) });

  useEffect(() => {
    if (!id) return;
    reservationsApi.getById(Number(id)).then((data) => {
      reset(data);
      setLoading(false);
    });
  }, [id, reset]);

  const onSubmit = async (values: ReservationFormValues) => {
    if (!id) return;
    setServerError(null);
    try {
      await reservationsApi.update(Number(id), { ...values, id: Number(id) } as any);
      navigate("/reservations");
    } catch {
      setServerError("Не вдалося оновити бронювання. Перевірте дані.");
    }
  };

  if (loading) return <p>Завантаження...</p>;

  return (
    <div>
      <h1>Редагувати бронювання</h1>
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        {serverError && <div className="alert alert-danger">{serverError}</div>}

        <div className="form-group">
          <label htmlFor="newGuestFirstName">Ім'я</label>
          <input id="newGuestFirstName" {...register("newGuestFirstName")} />
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
          {errors.guestsCount && <span className="field-error">{errors.guestsCount.message}</span>}
        </div>

        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Збереження..." : "Зберегти"}
        </button>
      </form>
    </div>
  );
}