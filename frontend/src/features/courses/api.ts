import { apiClient } from "../../lib/apiClient";
import type { CourseDto, CreateCourseRequest, UpdateCourseRequest } from "./types";

export const coursesApi = {
  list: async (): Promise<CourseDto[]> => {
    const { data } = await apiClient.get<CourseDto[]>("/courses");
    return data;
  },
  create: async (payload: CreateCourseRequest): Promise<CourseDto> => {
    const { data } = await apiClient.post<CourseDto>("/courses", payload);
    return data;
  },
  update: async (id: string, payload: UpdateCourseRequest): Promise<CourseDto> => {
    const { data } = await apiClient.put<CourseDto>(`/courses/${id}`, payload);
    return data;
  },
  remove: async (id: string): Promise<void> => {
    await apiClient.delete(`/courses/${id}`);
  },
};
