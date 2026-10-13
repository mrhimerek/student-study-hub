import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { examsApi } from "./api";
import type { CreateExamRequest, UpdateExamRequest } from "./types";

const EXAMS_ROOT_KEY = "exams";
const NEXT_EXAM_KEY = "exams-next";

export function useExams() {
  return useQuery({
    queryKey: [EXAMS_ROOT_KEY],
    queryFn: () => examsApi.list(),
  });
}

export function useNextExam() {
  return useQuery({
    queryKey: [NEXT_EXAM_KEY],
    queryFn: () => examsApi.getNext(),
  });
}

function invalidateExams(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: [EXAMS_ROOT_KEY] });
  queryClient.invalidateQueries({ queryKey: [NEXT_EXAM_KEY] });
}

export function useCreateExam() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateExamRequest) => examsApi.create(payload),
    onSuccess: () => invalidateExams(queryClient),
  });
}

export function useUpdateExam() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateExamRequest }) => examsApi.update(id, payload),
    onSuccess: () => invalidateExams(queryClient),
  });
}

export function useDeleteExam() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => examsApi.remove(id),
    onSuccess: () => invalidateExams(queryClient),
  });
}
