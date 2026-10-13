import { apiClient } from "../../lib/apiClient";
import type { ProfileDto, UpdateProfileRequest } from "./types";

export const profileApi = {
  get: async (): Promise<ProfileDto> => {
    const { data } = await apiClient.get<ProfileDto>("/users/me");
    return data;
  },
  update: async (payload: UpdateProfileRequest): Promise<ProfileDto> => {
    const { data } = await apiClient.put<ProfileDto>("/users/me", payload);
    return data;
  },
};
