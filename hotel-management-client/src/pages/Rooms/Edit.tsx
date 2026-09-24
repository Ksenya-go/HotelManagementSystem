import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate, useParams } from "react-router-dom";
import { roomsApi } from "@/api/roomsApi";
import { roomSchema, type RoomFormValues } from "@/schemas/roomSchema";
import { RoomOperationalStatus } from "@/types/enums";

export default function RoomEdit() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [serverError, setServerError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<RoomFormValues>({ resolver: zodResolver(roomSchema) });

  useEffect(() => {
    if (!id) return;
    roomsApi.getById(Number(id)).then((room) => {
      reset(room);
      setLoading(false);
    });
  }, [id, reset]);

  const onSubmit = async (values: RoomFormValues) => {
    if (!id) return;
    setServerError(null);
    try {
      await roomsApi.update(Number(id), { ...values, id: Number(id) });
      navigate("/rooms");
    } catch {
      setServerError("Не вдалося оновити номер. Перевірте дані.");
    }
  };

  if (loading) return <p>Завантаження...</p>;

  return (
    <div>
      <h1>Редагувати номер</h1>
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        {serverError && <div className="alert alert-danger">{serverError}</div>}

        <div className="form-group">
          <label htmlFor="roomNumber">Номер кімнати</label>
          <input id="roomNumber" {...register("roomNumber")} />
          {errors.roomNumber && <span className="field-error">{errors.roomNumber.message}</span>}
        </div>

        <div className="form-group">
          <label htmlFor="floor">Поверх</label>
          <input id="floor" type="number" {...register("floor", { valueAsNumber: true })} />
          {errors.floor && <span className="field-error">{errors.floor.message}</span>}
        </div>

        <div className="form-group">
          <label htmlFor="type">Тип кімнати</label>
          <input id="type" {...register("type")} />
          {errors.type && <span className="field-error">{errors.type.message}</span>}
        </div>

        <div className="form-group">
          <label htmlFor="description">Опис</label>
          <textarea id="description" {...register("description")} />
        </div>

        <div className="form-group">
          <label htmlFor="pricePerDay">Ціна за добу</label>
          <input
            id="pricePerDay"
            type="number"
            step="0.01"
            {...register("pricePerDay", { valueAsNumber: true })}
          />
          {errors.pricePerDay && <span className="field-error">{errors.pricePerDay.message}</span>}
        </div>

        <div className="form-group">
          <label htmlFor="capacity">Місткість номера</label>
          <input id="capacity" type="number" {...register("capacity", { valueAsNumber: true })} />
        </div>

        <div className="form-group">
          <label htmlFor="roomCount">Кількість кімнат</label>
          <input id="roomCount" type="number" {...register("roomCount", { valueAsNumber: true })} />
        </div>

        <div className="form-group">
          <label htmlFor="operationalStatus">Статус</label>
          <select id="operationalStatus" {...register("operationalStatus")}>
            <option value={RoomOperationalStatus.Clean}>Прибрано</option>
            <option value={RoomOperationalStatus.Cleaning}>Прибирається</option>
            <option value={RoomOperationalStatus.InMaintenance}>На обслуговуванні</option>
          </select>
        </div>

        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Збереження..." : "Зберегти"}
        </button>
      </form>
    </div>
  );
}