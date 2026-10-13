export interface AuthUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  gradeLevel: number;
  schoolId: string | null;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  gradeLevel: number;
  schoolId: string | null;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResult {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  user: AuthUser;
}

export interface ApiErrorBody {
  error?: string;
  errors?: string[];
}
