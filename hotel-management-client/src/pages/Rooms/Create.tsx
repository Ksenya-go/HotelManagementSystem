import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate } from "react-router-dom";
import { useState } from "react";
import { roomsApi } from "@/api/roomsApi";
import { roomSchema, roomDefaultValues, type RoomFormValues } from "@/schemas/roomSchema";
import { RoomOperationalStatus } from "@/types/enums";

export default function RoomCreate() {
  const navigate = useNavigate();
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<RoomFormValues>({
    resolver: zodResolver(roomSchema),
    defaultValues: roomDefaultValues,
  });

  const onSubmit = async (values: RoomFormValues) => {
    setServerError(null);
    try {
      await roomsApi.create(values);
      navigate("/rooms");
    } catch (err: unknown) {
      setServerError("Не вдалося створити номер. Перевірте дані.");
    }
  };

  return (
    <div>
      <h1>Додати номер</h1>
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
          {errors.description && <span className="field-error">{errors.description.message}</span>}
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
          {errors.capacity && <span className="field-error">{errors.capacity.message}</span>}
        </div>

        <div className="form-group">
          <label htmlFor="roomCount">Кількість кімнат</label>
          <input id="roomCount" type="number" {...register("roomCount", { valueAsNumber: true })} />
          {errors.roomCount && <span className="field-error">{errors.roomCount.message}</span>}
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
          {isSubmitting ? "Збереження..." : "Створити"}
        </button>
      </form>
    </div>
  );
}