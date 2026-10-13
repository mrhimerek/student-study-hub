export interface AuthUser {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  schoolId: string | null;
  gradeLevel: number;
  emailVerified: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  schoolId: string;
  gradeLevel: number;
}

export interface LoginResponse {
  accessToken: string;
  user: AuthUser;
}
