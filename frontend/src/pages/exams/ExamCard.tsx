import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import type { ExamDto, ExamType } from "../../features/exams/types";

const TYPE_LABELS: Record<ExamType, string> = {
  quiz: "Quiz",
  exam: "Exam",
  final: "Final",
};

const TYPE_BADGE_CLASSES: Record<ExamType, string> = {
  quiz: "bg-gray-100 text-gray-600",
  exam: "bg-blue-100 text-blue-700",
  final: "bg-rose-100 text-rose-700",
};

interface ExamCardProps {
  exam: ExamDto;
  onEdit: () => void;
  onDelete: () => void;
}

export function ExamCard({ exam, onEdit, onDelete }: ExamCardProps) {
  const examDate = new Date(exam.examDate);
  const dateLabel = examDate.toLocaleDateString(undefined, { month: "short", day: "numeric", year: "numeric" });

  return (
    <Card className="flex flex-col gap-2">
      <div className="flex items-start justify-between gap-2">
        <div>
          <p className="text-xs font-medium text-gray-400">{exam.subject}</p>
          <p className="text-sm font-semibold text-gray-800">{exam.title}</p>
        </div>
        <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${TYPE_BADGE_CLASSES[exam.type]}`}>
          {TYPE_LABELS[exam.type]}
        </span>
      </div>

      {exam.notes && <p className="text-xs text-gray-500">{exam.notes}</p>}

      <span className="text-xs text-gray-400">{dateLabel}</span>

      <div className="flex gap-2 pt-1">
        <Button variant="ghost" className="flex-1 px-2 py-1 text-xs" onClick={onEdit}>
          Edit
        </Button>
        <Button variant="ghost" className="flex-1 px-2 py-1 text-xs text-rose-600 hover:bg-rose-50" onClick={onDelete}>
          Delete
        </Button>
      </div>
    </Card>
  );
}

export { TYPE_LABELS };
