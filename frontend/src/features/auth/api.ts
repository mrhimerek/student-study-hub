import { apiClient } from "../../lib/apiClient";
import type { AuthResult, LoginRequest, RegisterRequest } from "./types";

export const authApi = {
  register: async (payload: RegisterRequest): Promise<AuthResult> => {
    const { data } = await apiClient.post<AuthResult>("/auth/register", payload);
    return data;
  },
  login: async (payload: LoginRequest): Promise<AuthResult> => {
    const { data } = await apiClient.post<AuthResult>("/auth/login", payload);
    return data;
  },
  logout: async (refreshToken: string): Promise<void> => {
    await apiClient.post("/auth/logout", { refreshToken });
  },
};
