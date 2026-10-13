import { apiClient } from "../../lib/apiClient";
import type {
  AssignmentDto,
  AssignmentStatus,
  CreateAssignmentRequest,
  UpdateAssignmentRequest,
} from "./types";

export const assignmentsApi = {
  list: async (status?: AssignmentStatus): Promise<AssignmentDto[]> => {
    const { data } = await apiClient.get<AssignmentDto[]>("/assignments", {
      params: status ? { status } : undefined,
    });
    return data;
  },
  create: async (payload: CreateAssignmentRequest): Promise<AssignmentDto> => {
    const { data } = await apiClient.post<AssignmentDto>("/assignments", payload);
    return data;
  },
  update: async (id: string, payload: UpdateAssignmentRequest): Promise<AssignmentDto> => {
    const { data } = await apiClient.put<AssignmentDto>(`/assignments/${id}`, payload);
    return data;
  },
  remove: async (id: string): Promise<void> => {
    await apiClient.delete(`/assignments/${id}`);
  },
};
