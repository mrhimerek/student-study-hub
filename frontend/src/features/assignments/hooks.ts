import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { assignmentsApi } from "./api";
import type { AssignmentStatus, CreateAssignmentRequest, UpdateAssignmentRequest } from "./types";

const ASSIGNMENTS_ROOT_KEY = "assignments";

function assignmentsKey(status?: AssignmentStatus) {
  return [ASSIGNMENTS_ROOT_KEY, status ?? "all"] as const;
}

export function useAssignments(status?: AssignmentStatus) {
  return useQuery({
    queryKey: assignmentsKey(status),
    queryFn: () => assignmentsApi.list(status),
  });
}

export function useCreateAssignment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateAssignmentRequest) => assignmentsApi.create(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [ASSIGNMENTS_ROOT_KEY] }),
  });
}

export function useUpdateAssignment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateAssignmentRequest }) =>
      assignmentsApi.update(id, payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [ASSIGNMENTS_ROOT_KEY] }),
  });
}

export function useDeleteAssignment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => assignmentsApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: [ASSIGNMENTS_ROOT_KEY] }),
  });
}
