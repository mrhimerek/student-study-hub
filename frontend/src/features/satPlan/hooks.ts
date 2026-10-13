import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { satPlanApi } from "./api";
import type { CreateSatPlanRequest, LogSatPracticeScoreRequest, UpdateSatPlanRequest } from "./types";

const SAT_PLAN_KEY = "sat-plan";

export function useSatPlan() {
  return useQuery({
    queryKey: [SAT_PLAN_KEY],
    queryFn: () => satPlanApi.get(),
  });
}

function invalidateSatPlan(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: [SAT_PLAN_KEY] });
}

export function useCreateSatPlan() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateSatPlanRequest) => satPlanApi.create(payload),
    onSuccess: () => invalidateSatPlan(queryClient),
  });
}

export function useUpdateSatPlan() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdateSatPlanRequest) => satPlanApi.update(payload),
    onSuccess: () => invalidateSatPlan(queryClient),
  });
}

export function useDeleteSatPlan() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => satPlanApi.remove(),
    onSuccess: () => invalidateSatPlan(queryClient),
  });
}

export function useLogSatPracticeScore() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: LogSatPracticeScoreRequest) => satPlanApi.logScore(payload),
    onSuccess: () => invalidateSatPlan(queryClient),
  });
}

export function useDeleteSatPracticeScore() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (scoreId: string) => satPlanApi.removeScore(scoreId),
    onSuccess: () => invalidateSatPlan(queryClient),
  });
}
