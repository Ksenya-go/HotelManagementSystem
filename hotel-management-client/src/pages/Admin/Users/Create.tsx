import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, useNavigate } from "react-router-dom";
import { usersApi } from "@/api/usersApi";
import { createUserSchema, type CreateUserValues } from "@/schemas/userSchema";
import { getApiError } from "@/utils/apiError";
import { roleLabels, USER_ROLES } from "@/types/user";

export default function AdminUserCreate() {
  const navigate = useNavigate();
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CreateUserValues>({
    resolver: zodResolver(createUserSchema),
    defaultValues: { fullName: "", email: "", password: "", role: "Employee" },
  });

  const onSubmit = async (values: CreateUserValues) => {
    setServerError(null);
    try {
      await usersApi.create(values);
      navigate("/admin/users", { state: { success: "Працівника створено." } });
    } catch (err) {
      setServerError(getApiError(err, "Не вдалося виконати операцію з користувачем."));
    }
  };

  return (
    <div>
      <div className="page-heading">
        <div>
          <h1>Додати працівника</h1>
          <p className="page-subtitle">Створіть обліковий запис і призначте рівень доступу.</p>
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
            <label htmlFor="password" className="form-label">Пароль</label>
            <input
              id="password"
              type="password"
              className="form-control"
              autoComplete="new-password"
              required
              {...register("password")}
            />
            {errors.password && <span className="text-danger">{errors.password.message}</span>}
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

          <div className="form-actions">
            <button type="submit" className="btn btn-teal" disabled={isSubmitting}>
              {isSubmitting ? "Збереження..." : "Створити працівника"}
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