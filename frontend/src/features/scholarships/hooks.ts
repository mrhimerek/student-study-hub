import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { scholarshipsApi } from "./api";
import type { CreateScholarshipRequest, UpdateScholarshipRequest } from "./types";

const SCHOLARSHIPS_KEY = "scholarships";

export function useScholarships() {
  return useQuery({
    queryKey: [SCHOLARSHIPS_KEY],
    queryFn: () => scholarshipsApi.list(),
  });
}

function invalidateScholarships(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: [SCHOLARSHIPS_KEY] });
}

export function useCreateScholarship() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateScholarshipRequest) => scholarshipsApi.create(payload),
    onSuccess: () => invalidateScholarships(queryClient),
  });
}

export function useUpdateScholarship() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateScholarshipRequest }) => scholarshipsApi.update(id, payload),
    onSuccess: () => invalidateScholarships(queryClient),
  });
}

export function useDeleteScholarship() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => scholarshipsApi.remove(id),
    onSuccess: () => invalidateScholarships(queryClient),
  });
}
