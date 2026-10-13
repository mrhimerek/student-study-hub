export type CalendarEventType = "assignment" | "exam" | "scholarship" | "collegeApplication" | "satTarget";

export interface CalendarEventDto {
  sourceId: string;
  type: CalendarEventType;
  title: string;
  subtitle: string | null;
  date: string;
}
