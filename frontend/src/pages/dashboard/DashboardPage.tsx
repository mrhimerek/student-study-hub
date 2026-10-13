import { Link } from "react-router-dom";
import { Card } from "../../components/ui/Card";
import { CountdownBadge } from "../../components/shared/CountdownBadge";
import { StatusChip } from "../../components/shared/StatusChip";
import { useAuth } from "../../features/auth/AuthContext";
import { useGpaSummary } from "../../features/gpa/hooks";
import { useNextExam } from "../../features/exams/hooks";

// TODO: replace with real data via TanStack Query hooks (features/assignments)
const mockDueToday = [
  { id: "1", title: "Math HW", priority: "High" as const },
  { id: "2", title: "History Essay", priority: "Medium" as const },
];

function daysUntil(isoDate: string): number {
  const msPerDay = 24 * 60 * 60 * 1000;
  const diff = new Date(isoDate).getTime() - Date.now();
  return Math.max(0, Math.ceil(diff / msPerDay));
}

export function DashboardPage() {
  const { user } = useAuth();
  const { data: gpaSummary, isLoading: isGpaLoading } = useGpaSummary();
  const { data: nextExam, isLoading: isExamLoading } = useNextExam();

  return (
    <div className="space-y-4">
      <p className="text-gray-600">Hi, {user?.firstName ?? "there"} 👋</p>

      <div className="grid grid-cols-2 gap-3">
        <Card>
          <p className="text-xs text-gray-500">GPA (weighted)</p>
          <p className="text-2xl font-semibold text-indigo-600">
            {isGpaLoading ? "…" : (gpaSummary?.weightedGpa ?? 0).toFixed(2)}
          </p>
          <Link to="/courses" className="text-xs font-medium text-indigo-500 hover:underline">
            Manage courses →
          </Link>
        </Card>
        <Card>
          <p className="text-xs text-gray-500">Next Exam</p>
          {isExamLoading ? (
            <p className="text-sm text-gray-400">Loading…</p>
          ) : nextExam ? (
            <>
              <p className="text-sm font-medium">{nextExam.title}</p>
              <CountdownBadge days={daysUntil(nextExam.examDate)} />
            </>
          ) : (
            <p className="text-sm text-gray-400">No exams scheduled</p>
          )}
          <Link to="/exams" className="text-xs font-medium text-indigo-500 hover:underline">
            Manage exams →
          </Link>
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
