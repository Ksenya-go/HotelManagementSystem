export interface LoginRequest {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface AuthUser {
  id: string;
  email: string;
  fullName: string;
  role: string; // "Admin" | "Manager" | "Receptionist" тощо
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: AuthUser;
}