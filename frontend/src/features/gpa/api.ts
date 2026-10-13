import { apiClient } from "../../lib/apiClient";
import type { GpaSummaryDto } from "./types";

export const gpaApi = {
  getSummary: async (): Promise<GpaSummaryDto> => {
    const { data } = await apiClient.get<GpaSummaryDto>("/gpa/summary");
    return data;
  },
};
