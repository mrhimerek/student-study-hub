import { apiClient } from "../../lib/apiClient";
import type { CreateExamRequest, ExamDto, UpdateExamRequest } from "./types";

export const examsApi = {
  list: async (): Promise<ExamDto[]> => {
    const { data } = await apiClient.get<ExamDto[]>("/exams");
    return data;
  },
  getNext: async (): Promise<ExamDto | null> => {
    const { data, status } = await apiClient.get<ExamDto | "">("/exams/next", {
      validateStatus: (s) => s === 200 || s === 204,
    });
    return status === 204 ? null : (data as ExamDto);
  },
  create: async (payload: CreateExamRequest): Promise<ExamDto> => {
    const { data } = await apiClient.post<ExamDto>("/exams", payload);
    return data;
  },
  update: async (id: string, payload: UpdateExamRequest): Promise<ExamDto> => {
    const { data } = await apiClient.put<ExamDto>(`/exams/${id}`, payload);
    return data;
  },
  remove: async (id: string): Promise<void> => {
    await apiClient.delete(`/exams/${id}`);
  },
};
