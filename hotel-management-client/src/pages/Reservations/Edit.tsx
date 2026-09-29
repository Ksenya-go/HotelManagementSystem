import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, useNavigate, useParams } from "react-router-dom";
import { reservationsApi } from "@/api/reservationsApi";
import { reservationSchema, type ReservationFormValues } from "@/schemas/reservationSchema";
import { getApiError } from "@/utils/apiError";
import { formatMoney } from "@/utils/format";

export default function ReservationEdit() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [serverError, setServerError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [readOnlyInfo, setReadOnlyInfo] = useState<{
    roomNumber: string;
    roomFloor: number;
    roomType: string;
    roomPricePerDay: number;
    roomCapacity: number;
  } | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ReservationFormValues>({ resolver: zodResolver(reservationSchema) });

  useEffect(() => {
    if (!id) return;
    reservationsApi
      .getById(Number(id))
      .then((data) => {
        reset({
          guestId: data.guestId,
          newGuestFirstName: data.guestFullName.split(" ")[0] ?? "",
          newGuestLastName: data.guestFullName.split(" ").slice(1).join(" "),
          newGuestEmail: data.guestEmail,
          newGuestPhone: data.guestPhone,
          roomId: 0,
          checkIn: data.checkIn,
          checkOut: data.checkOut,
          guestsCount: data.guestsCount,
        });
        setReadOnlyInfo({
          roomNumber: data.roomNumber,
          roomFloor: data.roomFloor,
          roomType: data.roomType,
          roomPricePerDay: data.roomPricePerDay,
          roomCapacity: data.roomCapacity,
        });
      })
      .catch((err) => setServerError(getApiError(err, "Бронювання не знайдено.")))
      .finally(() => setLoading(false));
  }, [id, reset]);

  const onSubmit = async (values: ReservationFormValues) => {
    if (!id) return;
    setServerError(null);
    try {
      await reservationsApi.update(Number(id), { ...values, id: Number(id) });
      navigate("/reservations", {
        state: { flash: { type: "success", text: "Бронювання оновлено." } },
      });
    } catch (err) {
      setServerError(getApiError(err, "Не вдалося оновити бронювання."));
    }
  };

  if (loading) return <p>Завантаження...</p>;

  return (
    <div>
      <div className="page-heading">
        <div>
          <h1>Редагувати бронювання</h1>
        </div>
      </div>

      <section className="content-card form-card">
        <form onSubmit={handleSubmit(onSubmit)} className="form-narrow">
          {serverError && <div className="validation-summary">{serverError}</div>}

          {readOnlyInfo && (
            <>
              <div className="form-field">
                <label className="form-label">Номер</label>
                <input
                  className="form-control form-control-readonly"
                  value={readOnlyInfo.roomNumber}
                  readOnly
                  tabIndex={-1}
                />
              </div>
              <div className="form-field">
                <label className="form-label">Поверх</label>
                <input
                  className="form-control form-control-readonly"
                  value={readOnlyInfo.roomFloor}
                  readOnly
                  tabIndex={-1}
                />
              </div>
              <div className="form-field">
                <label className="form-label">Тип номера</label>
                <input
                  className="form-control form-control-readonly"
                  value={readOnlyInfo.roomType}
                  readOnly
                  tabIndex={-1}
                />
              </div>
              <div className="form-field">
                <label className="form-label">Ціна за ніч (грн)</label>
                <input
                  className="form-control form-control-readonly"
                  value={formatMoney(readOnlyInfo.roomPricePerDay)}
                  readOnly
                  tabIndex={-1}
                />
              </div>
            </>
          )}

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

          <div className="form-field">
            <label htmlFor="checkIn" className="form-label">Дата заселення</label>
            <input id="checkIn" type="date" className="form-control" required {...register("checkIn")} />
            {errors.checkIn && <span className="text-danger">{errors.checkIn.message}</span>}
          </div>

          <div className="form-field">
            <label htmlFor="checkOut" className="form-label">Дата виселення</label>
            <input id="checkOut" type="date" className="form-control" required {...register("checkOut")} />
            {errors.checkOut && <span className="text-danger">{errors.checkOut.message}</span>}
          </div>

          <div className="form-field">
            <label htmlFor="guestsCount" className="form-label">Кількість гостей</label>
            <input
              id="guestsCount"
              type="number"
              min={1}
              max={20}
              className="form-control"
              required
              {...register("guestsCount", { valueAsNumber: true })}
            />
            {errors.guestsCount && <span className="text-danger">{errors.guestsCount.message}</span>}
          </div>

          <div className="form-actions">
            <button type="submit" className="btn btn-teal" disabled={isSubmitting}>
              Зберегти зміни
            </button>
            <Link to="/reservations" className="btn btn-cancel">
              Скасувати
            </Link>
          </div>
        </form>
      </section>
    </div>
  );
}