import { apiClient } from "../../lib/apiClient";
import type {
  CreateSatPlanRequest,
  LogSatPracticeScoreRequest,
  SatPlanDto,
  UpdateSatPlanRequest,
} from "./types";

export const satPlanApi = {
  get: async (): Promise<SatPlanDto | null> => {
    const { data, status } = await apiClient.get<SatPlanDto | "">("/sat-plan", {
      validateStatus: (s) => s === 200 || s === 204,
    });
    return status === 204 ? null : (data as SatPlanDto);
  },
  create: async (payload: CreateSatPlanRequest): Promise<SatPlanDto> => {
    const { data } = await apiClient.post<SatPlanDto>("/sat-plan", payload);
    return data;
  },
  update: async (payload: UpdateSatPlanRequest): Promise<SatPlanDto> => {
    const { data } = await apiClient.put<SatPlanDto>("/sat-plan", payload);
    return data;
  },
  remove: async (): Promise<void> => {
    await apiClient.delete("/sat-plan");
  },
  logScore: async (payload: LogSatPracticeScoreRequest): Promise<SatPlanDto> => {
    const { data } = await apiClient.post<SatPlanDto>("/sat-plan/scores", payload);
    return data;
  },
  removeScore: async (scoreId: string): Promise<SatPlanDto> => {
    const { data } = await apiClient.delete<SatPlanDto>(`/sat-plan/scores/${scoreId}`);
    return data;
  },
};
