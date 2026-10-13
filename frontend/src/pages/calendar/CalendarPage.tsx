import { useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { Card } from "../../components/ui/Card";
import { Button } from "../../components/ui/Button";
import { useCalendarEvents } from "../../features/calendar/hooks";
import type { CalendarEventDto, CalendarEventType } from "../../features/calendar/types";

const EVENT_STYLE: Record<CalendarEventType, { dot: string; label: string; link: string }> = {
  assignment: { dot: "bg-sky-500", label: "Assignment", link: "/assignments" },
  exam: { dot: "bg-purple-500", label: "Exam", link: "/exams" },
  scholarship: { dot: "bg-yellow-500", label: "Scholarship", link: "/scholarships" },
  collegeApplication: { dot: "bg-green-500", label: "College App", link: "/college-applications" },
  satTarget: { dot: "bg-rose-500", label: "SAT Target", link: "/sat-plan" },
};

const WEEKDAY_LABELS = ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"];

function toDateKey(date: Date): string {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, "0");
  const d = String(date.getDate()).padStart(2, "0");
  return `${y}-${m}-${d}`;
}

function buildMonthGrid(year: number, month: number): Date[] {
  const firstOfMonth = new Date(year, month, 1);
  const startOffset = firstOfMonth.getDay();

  const lastOfMonth = new Date(year, month + 1, 0);
  const endOffset = 6 - lastOfMonth.getDay();
  const totalDays = startOffset + lastOfMonth.getDate() + endOffset;

  return Array.from({ length: totalDays }, (_, i) => new Date(year, month, 1 - startOffset + i));
}

export function CalendarPage() {
  const [today] = useState(() => new Date());
  const [cursor, setCursor] = useState(() => new Date(today.getFullYear(), today.getMonth(), 1));
  const [selectedDate, setSelectedDate] = useState<string>(() => toDateKey(today));

  const year = cursor.getFullYear();
  const month = cursor.getMonth();

  const from = toDateKey(new Date(year, month, 1));
  const to = toDateKey(new Date(year, month + 1, 0));

  const { data: events, isLoading, isError } = useCalendarEvents(from, to);

  const eventsByDate = useMemo(() => {
    const map = new Map<string, CalendarEventDto[]>();
    for (const event of events ?? []) {
      const key = event.date.slice(0, 10);
      const list = map.get(key) ?? [];
      list.push(event);
      map.set(key, list);
    }
    return map;
  }, [events]);

  const days = useMemo(() => buildMonthGrid(year, month), [year, month]);
  const monthLabel = cursor.toLocaleDateString(undefined, { month: "long", year: "numeric" });
  const selectedEvents = eventsByDate.get(selectedDate) ?? [];

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-semibold text-gray-800">Calendar</h1>
      </div>

      <Card className="space-y-3">
        <div className="flex items-center justify-between">
          <Button variant="secondary" onClick={() => setCursor(new Date(year, month - 1, 1))}>
            ‹
          </Button>
          <span className="text-sm font-semibold text-gray-700">{monthLabel}</span>
          <Button variant="secondary" onClick={() => setCursor(new Date(year, month + 1, 1))}>
            ›
          </Button>
        </div>

        {isLoading && <p className="text-sm text-gray-400">Loading calendar…</p>}
        {isError && <p className="text-sm text-red-500">Couldn&apos;t load calendar events. Please try again.</p>}

        <div className="grid grid-cols-7 gap-1 text-center text-xs text-gray-400">
          {WEEKDAY_LABELS.map((label) => (
            <span key={label}>{label}</span>
          ))}
        </div>

        <div className="grid grid-cols-7 gap-1">
          {days.map((day) => {
            const key = toDateKey(day);
            const inMonth = day.getMonth() === month;
            const dayEvents = eventsByDate.get(key) ?? [];
            const dotTypes = Array.from(new Set(dayEvents.map((e) => e.type)));
            const isSelected = key === selectedDate;
            const isToday = key === toDateKey(today);

            return (
              <button
                key={key}
                onClick={() => setSelectedDate(key)}
                className={`flex h-14 flex-col items-center justify-start rounded-lg border p-1 text-xs ${
                  inMonth ? "text-gray-700" : "text-gray-300"
                } ${isSelected ? "border-indigo-500 bg-indigo-50" : "border-transparent hover:bg-gray-50"}`}
              >
                <span className={isToday ? "rounded-full bg-indigo-600 px-1.5 text-white" : ""}>{day.getDate()}</span>
                <span className="mt-1 flex gap-0.5">
                  {dotTypes.map((type) => (
                    <span key={type} className={`h-1.5 w-1.5 rounded-full ${EVENT_STYLE[type].dot}`} />
                  ))}
                </span>
              </button>
            );
          })}
        </div>

        <div className="flex flex-wrap gap-3 border-t border-gray-100 pt-2 text-xs text-gray-500">
          {(Object.keys(EVENT_STYLE) as CalendarEventType[]).map((type) => (
            <span key={type} className="flex items-center gap-1">
              <span className={`h-1.5 w-1.5 rounded-full ${EVENT_STYLE[type].dot}`} />
              {EVENT_STYLE[type].label}
            </span>
          ))}
        </div>
      </Card>

      <section>
        <h2 className="mb-2 text-sm font-semibold text-gray-700">
          {new Date(`${selectedDate}T00:00:00`).toLocaleDateString(undefined, {
            weekday: "long",
            month: "long",
            day: "numeric",
          })}
        </h2>
        {selectedEvents.length === 0 ? (
          <p className="text-sm text-gray-400">No events on this day.</p>
        ) : (
          <div className="space-y-2">
            {selectedEvents.map((event) => (
              <Card key={`${event.type}-${event.sourceId}`} className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <span className={`h-2 w-2 rounded-full ${EVENT_STYLE[event.type].dot}`} />
                  <div>
                    <p className="text-sm font-medium">{event.title}</p>
                    {event.subtitle && <p className="text-xs text-gray-500">{event.subtitle}</p>}
                  </div>
                </div>
                <Link to={EVENT_STYLE[event.type].link} className="text-xs font-medium text-indigo-500 hover:underline">
                  View →
                </Link>
              </Card>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
