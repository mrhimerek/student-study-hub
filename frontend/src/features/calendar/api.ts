import { apiClient } from "../../lib/apiClient";
import type { CalendarEventDto } from "./types";

export const calendarApi = {
  getEvents: async (from: string, to: string): Promise<CalendarEventDto[]> => {
    const { data } = await apiClient.get<CalendarEventDto[]>("/calendar", {
      params: { from, to },
    });
    return data;
  },
};
