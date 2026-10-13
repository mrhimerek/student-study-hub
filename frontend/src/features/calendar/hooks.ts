import { useQuery } from "@tanstack/react-query";
import { calendarApi } from "./api";

/** from/to are "yyyy-MM-dd" date-only strings. */
export function useCalendarEvents(from: string, to: string) {
  return useQuery({
    queryKey: ["calendar", from, to],
    queryFn: () => calendarApi.getEvents(from, to),
  });
}
