import { apiClient } from "../../lib/apiClient";
import type { CollegeApplicationDto, CreateCollegeApplicationRequest, UpdateCollegeApplicationRequest } from "./types";

export const collegeApplicationsApi = {
  list: async (): Promise<CollegeApplicationDto[]> => {
    const { data } = await apiClient.get<CollegeApplicationDto[]>("/college-applications");
    return data;
  },
  create: async (payload: CreateCollegeApplicationRequest): Promise<CollegeApplicationDto> => {
    const { data } = await apiClient.post<CollegeApplicationDto>("/college-applications", payload);
    return data;
  },
  update: async (id: string, payload: UpdateCollegeApplicationRequest): Promise<CollegeApplicationDto> => {
    const { data } = await apiClient.put<CollegeApplicationDto>(`/college-applications/${id}`, payload);
    return data;
  },
  remove: async (id: string): Promise<void> => {
    await apiClient.delete(`/college-applications/${id}`);
  },
};
