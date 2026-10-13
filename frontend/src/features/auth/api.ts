import { apiClient } from "../../lib/apiClient";
import type { LoginRequest, LoginResponse, RegisterRequest, AuthUser } from "./types";

export const authApi = {
  login: async (payload: LoginRequest): Promise<LoginResponse> => {
    const { data } = await apiClient.post<LoginResponse>("/auth/login", payload);
    return data;
  },
  register: async (payload: RegisterRequest): Promise<void> => {
    await apiClient.post("/auth/register", payload);
  },
  logout: async (): Promise<void> => {
    await apiClient.post("/auth/logout");
  },
  me: async (): Promise<AuthUser> => {
    const { data } = await apiClient.get<AuthUser>("/users/me");
    return data;
  },
};
