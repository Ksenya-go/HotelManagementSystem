import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, Navigate, useNavigate, useSearchParams } from "react-router-dom";
import { reservationsApi } from "@/api/reservationsApi";
import { roomsApi } from "@/api/roomsApi";
import { reservationSchema, type ReservationFormValues } from "@/schemas/reservationSchema";
import type { RoomEditInput } from "@/types/room";
import { getApiError } from "@/utils/apiError";
import { formatMoney } from "@/utils/format";

export default function ReservationCreate() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const roomId = searchParams.get("roomId");
  const checkIn = searchParams.get("checkIn") ?? "";
  const checkOut = searchParams.get("checkOut") ?? "";
  const guestsCount = Math.max(1, Number(searchParams.get("guestsCount") ?? 1));

  const [room, setRoom] = useState<RoomEditInput | null>(null);
  const [loading, setLoading] = useState(true);
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
      roomId: roomId ? Number(roomId) : 0,
      checkIn,
      checkOut,
      guestsCount,
    },
  });

  useEffect(() => {
    if (!roomId) return;
    roomsApi
      .getById(Number(roomId))
      .then(setRoom)
      .catch(() => setServerError("Номер не знайдено."))
      .finally(() => setLoading(false));
  }, [roomId]);

  if (!roomId || !checkIn || !checkOut) {
    return (
      <Navigate
        to={`/rooms/booking?startDate=${checkIn}&endDate=${checkOut}&guestsCount=${guestsCount}`}
        replace
      />
    );
  }

  const onSubmit = async (values: ReservationFormValues) => {
    setServerError(null);
    try {
      await reservationsApi.create(values);
      navigate("/reservations", {
        state: { flash: { type: "success", text: "Бронювання створено." } },
      });
    } catch (err) {
      setServerError(getApiError(err, "Не вдалося створити бронювання."));
    }
  };

  if (loading) return <p>Завантаження...</p>;

  return (
    <div>
      <div className="page-heading">
        <div>
          <h1>Нове бронювання</h1>
        </div>
      </div>

      <section className="content-card form-card">
        <form onSubmit={handleSubmit(onSubmit)} className="form-narrow">
          {serverError && <div className="validation-summary">{serverError}</div>}

          <input type="hidden" {...register("roomId", { valueAsNumber: true })} />
          <input type="hidden" {...register("checkIn")} />
          <input type="hidden" {...register("checkOut")} />

          <div className="form-field">
            <label className="form-label">Обраний номер</label>
            <input
              className="form-control form-control-readonly"
              value={room?.roomNumber ?? ""}
              readOnly
              tabIndex={-1}
            />
          </div>

          <div className="form-field">
            <label className="form-label">Поверх</label>
            <input
              className="form-control form-control-readonly"
              value={room?.floor ?? ""}
              readOnly
              tabIndex={-1}
            />
          </div>

          <div className="form-field">
            <label className="form-label">Тип номера</label>
            <input
              className="form-control form-control-readonly"
              value={room?.type ?? ""}
              readOnly
              tabIndex={-1}
            />
          </div>

          <div className="form-field">
            <label className="form-label">Ціна за ніч (грн)</label>
            <input
              className="form-control form-control-readonly"
              value={room ? formatMoney(room.pricePerDay) : ""}
              readOnly
              tabIndex={-1}
            />
          </div>

          <div className="form-field">
            <label className="form-label">Місткість номера</label>
            <input
              className="form-control form-control-readonly"
              value={room?.capacity ?? ""}
              readOnly
              tabIndex={-1}
            />
          </div>

          <div className="form-field">
            <label className="form-label">Заселення</label>
            <input className="form-control form-control-readonly" value={checkIn} readOnly tabIndex={-1} />
          </div>

          <div className="form-field">
            <label className="form-label">Виселення</label>
            <input className="form-control form-control-readonly" value={checkOut} readOnly tabIndex={-1} />
          </div>

          <div className="form-field">
            <label htmlFor="newGuestFirstName" className="form-label">Ім'я</label>
            <input id="newGuestFirstName" className="form-control" required {...register("newGuestFirstName")} />
            {errors.newGuestFirstName && (
              <span className="text-danger">{errors.newGuestFirstName.message}</span>
            )}
          </div>

          <div className="form-field">
            <label htmlFor="newGuestLastName" className="form-label">Прізвище</label>
            <input id="newGuestLastName" className="form-control" required {...register("newGuestLastName")} />
            {errors.newGuestLastName && (
              <span className="text-danger">{errors.newGuestLastName.message}</span>
            )}
          </div>

          <div className="form-field">
            <label htmlFor="newGuestEmail" className="form-label">Електронна пошта</label>
            <input id="newGuestEmail" type="email" className="form-control" required {...register("newGuestEmail")} />
            {errors.newGuestEmail && <span className="text-danger">{errors.newGuestEmail.message}</span>}
          </div>

          <div className="form-field">
            <label htmlFor="newGuestPhone" className="form-label">Телефон</label>
            <input id="newGuestPhone" className="form-control" required {...register("newGuestPhone")} />
            {errors.newGuestPhone && <span className="text-danger">{errors.newGuestPhone.message}</span>}
          </div>

          <div className="form-actions">
            <button type="submit" className="btn btn-teal" disabled={isSubmitting}>
              Забронювати
            </button>
            <Link
              to={`/rooms/booking?startDate=${checkIn}&endDate=${checkOut}&guestsCount=${guestsCount}`}
              className="btn btn-cancel"
            >
              Скасувати
            </Link>
          </div>
        </form>
      </section>
    </div>
  );
}