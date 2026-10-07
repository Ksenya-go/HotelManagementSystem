import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from "react";
import httpClient from "@/api/httpClient";
import type { AuthUser, LoginRequest, LoginResponse } from "@/types/auth";

interface AuthContextValue {
  user: AuthUser | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (data: LoginRequest) => Promise<void>;
  logout: () => Promise<void>;
  hasRole: (...roles: string[]) => boolean;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    let cancelled = false;

    httpClient
      .get<AuthUser>("/account/me")
      .then((response) => {
        if (!cancelled) setUser(response.data);
      })
      .catch(() => {
        if (!cancelled) setUser(null);
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, []);

  const login = useCallback(async (data: LoginRequest) => {
    const response = await httpClient.post<LoginResponse>("/account/login", data);
    setUser(response.data.user); // токен сервер поклав у cookie сам
  }, []);

  const logout = useCallback(async () => {
    try {
      await httpClient.post("/account/logout");
    } finally {
      setUser(null);
    }
  }, []);

  const hasRole = useCallback(
    (...roles: string[]) => !!user && roles.includes(user.role),
    [user]
  );

  return (
    <AuthContext.Provider
      value={{ user, isAuthenticated: !!user, isLoading, login, logout, hasRole }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth має використовуватись всередині <AuthProvider>");
  return ctx;
}