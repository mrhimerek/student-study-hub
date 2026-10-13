import { apiClient } from "../../lib/apiClient";
import type { CreateScholarshipRequest, ScholarshipDto, UpdateScholarshipRequest } from "./types";

export const scholarshipsApi = {
  list: async (): Promise<ScholarshipDto[]> => {
    const { data } = await apiClient.get<ScholarshipDto[]>("/scholarships");
    return data;
  },
  create: async (payload: CreateScholarshipRequest): Promise<ScholarshipDto> => {
    const { data } = await apiClient.post<ScholarshipDto>("/scholarships", payload);
    return data;
  },
  update: async (id: string, payload: UpdateScholarshipRequest): Promise<ScholarshipDto> => {
    const { data } = await apiClient.put<ScholarshipDto>(`/scholarships/${id}`, payload);
    return data;
  },
  remove: async (id: string): Promise<void> => {
    await apiClient.delete(`/scholarships/${id}`);
  },
};
