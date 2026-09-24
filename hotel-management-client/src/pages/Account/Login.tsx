import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate, useLocation } from "react-router-dom";
import { useState } from "react";
import { useAuth } from "@/context/AuthContext";
import { loginSchema, type LoginFormValues } from "@/schemas/loginSchema";

export default function Login() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [serverError, setServerError] = useState<string | null>(null);

  const returnUrl = (location.state as { from?: Location })?.from?.pathname ?? "/";

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "", rememberMe: false },
  });

  const onSubmit = async (values: LoginFormValues) => {
    setServerError(null);
    try {
      await login(values);
      navigate(returnUrl, { replace: true });
    } catch {
      setServerError("Невірна електронна пошта або пароль.");
    }
  };

  return (
    <div className="login-page">
      <h1>Вхід</h1>

      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        {serverError && <div className="alert alert-danger">{serverError}</div>}

        <div className="form-group">
          <label htmlFor="email">Електронна пошта</label>
          <input id="email" type="email" {...register("email")} />
          {errors.email && <span className="field-error">{errors.email.message}</span>}
        </div>

        <div className="form-group">
          <label htmlFor="password">Пароль</label>
          <input id="password" type="password" {...register("password")} />
          {errors.password && <span className="field-error">{errors.password.message}</span>}
        </div>

        <div className="form-check">
          <input id="rememberMe" type="checkbox" {...register("rememberMe")} />
          <label htmlFor="rememberMe">Запам'ятати мене</label>
        </div>

        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Вхід..." : "Увійти"}
        </button>
      </form>
    </div>
  );
}