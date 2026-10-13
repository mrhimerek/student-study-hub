import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { coursesApi } from "./api";
import type { CreateCourseRequest, UpdateCourseRequest } from "./types";

const COURSES_ROOT_KEY = "courses";
const GPA_ROOT_KEY = "gpa";

export function useCourses() {
  return useQuery({
    queryKey: [COURSES_ROOT_KEY],
    queryFn: () => coursesApi.list(),
  });
}

function invalidateCoursesAndGpa(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: [COURSES_ROOT_KEY] });
  // Courses are the source data for the GPA summary, so any course mutation
  // must also invalidate the cached GPA summary.
  queryClient.invalidateQueries({ queryKey: [GPA_ROOT_KEY] });
}

export function useCreateCourse() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateCourseRequest) => coursesApi.create(payload),
    onSuccess: () => invalidateCoursesAndGpa(queryClient),
  });
}

export function useUpdateCourse() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateCourseRequest }) => coursesApi.update(id, payload),
    onSuccess: () => invalidateCoursesAndGpa(queryClient),
  });
}

export function useDeleteCourse() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => coursesApi.remove(id),
    onSuccess: () => invalidateCoursesAndGpa(queryClient),
  });
}
