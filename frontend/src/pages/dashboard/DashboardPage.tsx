import { Card } from "../../components/ui/Card";
import { CountdownBadge } from "../../components/shared/CountdownBadge";
import { StatusChip } from "../../components/shared/StatusChip";

// TODO: replace with real data via TanStack Query hooks (features/assignments, features/exams, etc.)
const mockDueToday = [
  { id: "1", title: "Math HW", priority: "High" as const },
  { id: "2", title: "History Essay", priority: "Medium" as const },
];

const mockNextExam = { title: "Biology Final", daysLeft: 2 };

export function DashboardPage() {
  return (
    <div className="space-y-4">
      <p className="text-gray-600">Hi, Priya 👋</p>

      <div className="grid grid-cols-2 gap-3">
        <Card>
          <p className="text-xs text-gray-500">GPA (weighted)</p>
          <p className="text-2xl font-semibold text-indigo-600">3.8</p>
        </Card>
        <Card>
          <p className="text-xs text-gray-500">Next Exam</p>
          <p className="text-sm font-medium">{mockNextExam.title}</p>
          <CountdownBadge days={mockNextExam.daysLeft} />
        </Card>
      </div>

      <section>
        <h2 className="mb-2 text-sm font-semibold text-gray-700">📌 Due Today</h2>
        <div className="space-y-2">
          {mockDueToday.map((item) => (
            <Card key={item.id} className="flex items-center justify-between">
              <span className="text-sm">{item.title}</span>
              <StatusChip priority={item.priority} />
            </Card>
          ))}
        </div>
      </section>
    </div>
  );
}
