import { useState } from "react";
import { useForm } from "react-hook-form";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";
import { getApiError } from "@/utils/apiError";

interface LoginFormValues {
  email: string;
  password: string;
  rememberMe: boolean;
}

export default function Login() {
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: { pathname: string } } | null)?.from?.pathname;

  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({
    defaultValues: { email: "", password: "", rememberMe: false },
  });

  if (isAuthenticated) {
    return <Navigate to={from ?? "/reservations"} replace />;
  }

  const onSubmit = async (values: LoginFormValues) => {
    setServerError(null);
    try {
      await login(values);
      navigate(from ?? "/reservations", { replace: true });
    } catch (err) {
      setServerError(getApiError(err, "Невірний email або пароль."));
    }
  };

  return (
    <div className="login-panel-standalone">
      <div className="login-form-wrap">
        <div className="brand-lockup login-brand login-brand-standalone">
          <div className="brand-mark">Г</div>
          <div>
            <strong>
              Готель<span>Плюс</span>
            </strong>
          </div>
        </div>

        <h1>Увійдіть у системи</h1>
        <p className="page-subtitle">Введіть дані для входу до системи.</p>

        <form onSubmit={handleSubmit(onSubmit)} noValidate>
          {serverError && <div className="validation-summary">{serverError}</div>}

          <div className="form-field">
            <label htmlFor="email" className="form-label">Електронна пошта</label>
            <input
              id="email"
              type="email"
              className="form-control"
              autoComplete="username"
              placeholder="Введіть електронну пошту"
              {...register("email", {
                required: "Вкажіть електронну пошту.",
                pattern: {
                  value: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
                  message: "Вкажіть коректну електронну пошту.",
                },
              })}
            />
            {errors.email && <span className="text-danger">{errors.email.message}</span>}
          </div>

          <div className="form-field">
            <label htmlFor="password" className="form-label">Пароль</label>
            <input
              id="password"
              type="password"
              className="form-control"
              autoComplete="current-password"
              placeholder="Введіть пароль"
              {...register("password", { required: "Вкажіть пароль." })}
            />
            {errors.password && <span className="text-danger">{errors.password.message}</span>}
          </div>

          <div className="login-options">
            <input
              id="rememberMe"
              type="checkbox"
              className="form-check-input"
              {...register("rememberMe")}
            />
            <label htmlFor="rememberMe" className="form-check-label">
              Запам’ятати мене
            </label>
          </div>

          <button type="submit" className="btn btn-teal btn-login" disabled={isSubmitting}>
            {isSubmitting ? "Вхід..." : "Увійти"} <span>→</span>
          </button>
        </form>
      </div>
    </div>
  );
}