import type { FieldErrors, UseFormRegister } from "react-hook-form";
import type { RoomFormValues } from "@/schemas/roomSchema";

interface RoomFormFieldsProps {
  register: UseFormRegister<RoomFormValues>;
  errors: FieldErrors<RoomFormValues>;
  withFloorPlaceholder?: boolean;
}

export default function RoomFormFields({
  register,
  errors,
  withFloorPlaceholder = false,
}: RoomFormFieldsProps) {
  return (
    <>
      <div className="form-field">
        <label htmlFor="roomNumber" className="form-label">Номер кімнати</label>
        <input id="roomNumber" className="form-control" required {...register("roomNumber")} />
        {errors.roomNumber && <span className="text-danger">{errors.roomNumber.message}</span>}
      </div>

      <div className="form-field">
        <label htmlFor="type" className="form-label">Тип номера</label>
        <input id="type" className="form-control" required {...register("type")} />
        {errors.type && <span className="text-danger">{errors.type.message}</span>}
      </div>

      <div className="form-field">
        <label htmlFor="description" className="form-label">Опис</label>
        <textarea
          id="description"
          className="form-control"
          rows={3}
          required
          {...register("description")}
        />
        {errors.description && <span className="text-danger">{errors.description.message}</span>}
      </div>

      <div className="form-field">
        <label htmlFor="pricePerDay" className="form-label">Ціна за ніч (грн)</label>
        <input
          id="pricePerDay"
          type="number"
          step="0.01"
          className="form-control"
          required
          {...register("pricePerDay", { valueAsNumber: true })}
        />
        {errors.pricePerDay && <span className="text-danger">{errors.pricePerDay.message}</span>}
      </div>

      <div className="form-field">
        <label htmlFor="capacity" className="form-label">Місткість номера</label>
        <input
          id="capacity"
          type="number"
          min={1}
          max={20}
          className="form-control"
          required
          {...register("capacity", { valueAsNumber: true })}
        />
        {errors.capacity && <span className="text-danger">{errors.capacity.message}</span>}
      </div>

      <div className="form-field">
        <label htmlFor="roomCount" className="form-label">Кількість кімнат</label>
        <input
          id="roomCount"
          type="number"
          min={1}
          max={20}
          className="form-control"
          required
          {...register("roomCount", { valueAsNumber: true })}
        />
        {errors.roomCount && <span className="text-danger">{errors.roomCount.message}</span>}
      </div>

      <div className="form-field">
        <label htmlFor="floor" className="form-label">Поверх</label>
        <select
          id="floor"
          className="form-select"
          required
          {...register("floor", { valueAsNumber: true })}
        >
          {withFloorPlaceholder && <option value="">Виберіть поверх</option>}
          {Array.from({ length: 9 }, (_, i) => i + 1).map((floor) => (
            <option key={floor} value={floor}>
              {floor}
            </option>
          ))}
        </select>
        {errors.floor && <span className="text-danger">{errors.floor.message}</span>}
      </div>

      <div className="form-field">
        <label htmlFor="operationalStatus" className="form-label">Статус кімнати</label>
        <select
          id="operationalStatus"
          className="form-select"
          required
          {...register("operationalStatus")}
        >
          <option value="Clean">Доступний</option>
          <option value="Cleaning">Потребує прибирання</option>
          <option value="InMaintenance">На обслуговуванні</option>
        </select>
      </div>
    </>
  );
}