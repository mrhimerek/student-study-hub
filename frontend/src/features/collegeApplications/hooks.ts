import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { collegeApplicationsApi } from "./api";
import type { CreateCollegeApplicationRequest, UpdateCollegeApplicationRequest } from "./types";

const COLLEGE_APPLICATIONS_KEY = "college-applications";

export function useCollegeApplications() {
  return useQuery({
    queryKey: [COLLEGE_APPLICATIONS_KEY],
    queryFn: () => collegeApplicationsApi.list(),
  });
}

function invalidateCollegeApplications(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: [COLLEGE_APPLICATIONS_KEY] });
}

export function useCreateCollegeApplication() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateCollegeApplicationRequest) => collegeApplicationsApi.create(payload),
    onSuccess: () => invalidateCollegeApplications(queryClient),
  });
}

export function useUpdateCollegeApplication() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateCollegeApplicationRequest }) =>
      collegeApplicationsApi.update(id, payload),
    onSuccess: () => invalidateCollegeApplications(queryClient),
  });
}

export function useDeleteCollegeApplication() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => collegeApplicationsApi.remove(id),
    onSuccess: () => invalidateCollegeApplications(queryClient),
  });
}
