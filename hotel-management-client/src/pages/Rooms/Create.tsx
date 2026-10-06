import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, useNavigate } from "react-router-dom";
import { roomsApi } from "@/api/roomsApi";
import RoomFormFields from "@/components/RoomFormFields";
import { roomDefaultValues, roomSchema, type RoomFormValues } from "@/schemas/roomSchema";
import { getApiError } from "@/utils/apiError";

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
      navigate("/rooms", { state: { flash: { type: "success", text: "Кімнати додано." } } });
    } catch (err) {
      setServerError(getApiError(err, "Не вдалося виконати операцію з номером."));
    }
  };

  return (
    <div>
      <div className="page-heading">
        <div>
          <h1>Додати кімнату</h1>
        </div>
      </div>

      <section className="content-card form-card">
        <form onSubmit={handleSubmit(onSubmit)} className="form-narrow">
          {serverError && <div className="validation-summary">{serverError}</div>}

          <RoomFormFields register={register} errors={errors} withFloorPlaceholder />

          <div className="form-actions">
            <button type="submit" className="btn btn-teal" disabled={isSubmitting}>
              Додати кімнату
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