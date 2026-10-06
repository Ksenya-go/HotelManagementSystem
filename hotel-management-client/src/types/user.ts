export interface UserRowDto {
  id: string;
  email: string;
  fullName: string;
  role: string; // "Employee" | "Admin"
  isLocked: boolean;
}

export interface UserDetailsDto {
  id: string;
  fullName: string;
  email: string;
  role: string;
}

export interface CreateUserInput {
  fullName: string;
  email: string;
  password: string;
  role: string;
}

export interface UpdateUserInput {
  fullName: string;
  email: string;
  role: string;
  newPassword?: string;
}

export const USER_ROLES = ["Employee", "Admin"] as const;

export const roleLabels: Record<string, string> = {
  Employee: "Працівник готелю",
  Admin: "Системний адміністратор",
};