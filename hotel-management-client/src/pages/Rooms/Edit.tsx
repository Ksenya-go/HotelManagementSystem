import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, useNavigate, useParams } from "react-router-dom";
import { roomsApi } from "@/api/roomsApi";
import RoomFormFields from "@/components/RoomFormFields";
import { roomDefaultValues, roomSchema, type RoomFormValues } from "@/schemas/roomSchema";
import { getApiError } from "@/utils/apiError";

export default function RoomEdit() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<RoomFormValues>({
    resolver: zodResolver(roomSchema),
    defaultValues: roomDefaultValues,
  });

  useEffect(() => {
    if (!id) return;
    roomsApi
      .getById(Number(id))
      .then((room) =>
        reset({
          roomNumber: room.roomNumber,
          floor: room.floor,
          type: room.type,
          description: room.description ?? "",
          pricePerDay: room.pricePerDay,
          capacity: room.capacity,
          roomCount: room.roomCount,
          operationalStatus: room.operationalStatus,
        })
      )
      .catch((err) => setServerError(getApiError(err, "Не вдалося виконати операцію з номером.")))
      .finally(() => setLoading(false));
  }, [id, reset]);

  const onSubmit = async (values: RoomFormValues) => {
    if (!id) return;
    setServerError(null);
    try {
      await roomsApi.update(Number(id), { ...values, id: Number(id) });
      navigate("/rooms", { state: { flash: { type: "success", text: "Номер оновлено." } } });
    } catch (err) {
      setServerError(getApiError(err, "Не вдалося виконати операцію з номером."));
    }
  };

  if (loading) return <p>Завантаження...</p>;

  return (
    <div>
      <div className="page-heading">
        <div>
          <h1>Редагувати номер</h1>
        </div>
      </div>

      <section className="content-card form-card">
        <form onSubmit={handleSubmit(onSubmit)} className="form-narrow">
          {serverError && <div className="validation-summary">{serverError}</div>}

          <RoomFormFields register={register} errors={errors} />

          <div className="form-actions">
            <button type="submit" className="btn btn-teal" disabled={isSubmitting}>
              Зберегти зміни
            </button>
            <Link to="/rooms" className="btn btn-cancel">
              Скасувати
            </Link>
          </div>
        </form>
      </section>
    </div>
  );
}