import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, useNavigate, useParams } from "react-router-dom";
import { usersApi } from "@/api/usersApi";
import { editUserSchema, type EditUserValues } from "@/schemas/userSchema";
import { getApiError } from "@/utils/apiError";
import { roleLabels, USER_ROLES } from "@/types/user";

export default function AdminUserEdit() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<EditUserValues>({
    resolver: zodResolver(editUserSchema),
    defaultValues: { fullName: "", email: "", role: "Employee", newPassword: "" },
  });

  useEffect(() => {
    if (!id) return;
    usersApi
      .getById(id)
      .then((u) =>
        reset({ fullName: u.fullName, email: u.email, role: u.role, newPassword: "" })
      )
      .catch(() => setServerError("Користувача не знайдено."))
      .finally(() => setLoading(false));
  }, [id, reset]);

  const onSubmit = async (values: EditUserValues) => {
    if (!id) return;
    setServerError(null);
    try {
      await usersApi.update(id, {
        fullName: values.fullName,
        email: values.email,
        role: values.role,
        newPassword: values.newPassword ? values.newPassword : undefined,
      });
      navigate("/admin/users", { state: { success: "Працівника оновлено." } });
    } catch (err) {
      setServerError(getApiError(err, "Не вдалося виконати операцію з користувачем."));
    }
  };

  if (loading) return <p>Завантаження...</p>;

  return (
    <div>
      <div className="page-heading">
        <div>
          <h1>Редагувати працівника</h1>
          <p className="page-subtitle">Редагувати обліковий запис</p>
        </div>
      </div>

      <section className="content-card form-card">
        <form onSubmit={handleSubmit(onSubmit)} className="form-narrow" noValidate>
          {serverError && <div className="validation-summary">{serverError}</div>}

          <div className="form-field">
            <label htmlFor="fullName" className="form-label">ПІБ</label>
            <input id="fullName" className="form-control" required {...register("fullName")} />
            {errors.fullName && <span className="text-danger">{errors.fullName.message}</span>}
          </div>

          <div className="form-field">
            <label htmlFor="email" className="form-label">Електронна пошта</label>
            <input
              id="email"
              type="email"
              className="form-control"
              autoComplete="username"
              required
              {...register("email")}
            />
            {errors.email && <span className="text-danger">{errors.email.message}</span>}
          </div>

          <div className="form-field">
            <label htmlFor="role" className="form-label">Роль</label>
            <select id="role" className="form-select" required {...register("role")}>
             {USER_ROLES.map((r: (typeof USER_ROLES)[number]) => (
                <option key={r} value={r}>
                  {roleLabels[r]}
                </option>
              ))}
            </select>
            {errors.role && <span className="text-danger">{errors.role.message}</span>}
          </div>

          <div className="form-field">
            <label htmlFor="newPassword" className="form-label">Новий пароль</label>
            <input
              id="newPassword"
              type="password"
              className="form-control"
              autoComplete="new-password"
              aria-describedby="new-password-help"
              {...register("newPassword")}
            />
            <div id="new-password-help" className="form-text">
              Залиште поле порожнім, якщо не потрібно змінювати пароль.
            </div>
            {errors.newPassword && (
              <span className="text-danger">{errors.newPassword.message}</span>
            )}
          </div>

          <div className="form-actions">
            <button type="submit" className="btn btn-teal" disabled={isSubmitting}>
              {isSubmitting ? "Збереження..." : "Зберегти зміни"}
            </button>
            <Link to="/admin/users" className="btn btn-cancel">
              Скасувати
            </Link>
          </div>
        </form>
      </section>
    </div>
  );
}