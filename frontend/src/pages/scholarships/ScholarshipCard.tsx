import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import type { ScholarshipDto, ScholarshipStatus } from "../../features/scholarships/types";

const STATUS_LABELS: Record<ScholarshipStatus, string> = {
  researching: "Researching",
  applying: "Applying",
  submitted: "Submitted",
  awarded: "Awarded",
  rejected: "Rejected",
};

const STATUS_BADGE_CLASSES: Record<ScholarshipStatus, string> = {
  researching: "bg-gray-100 text-gray-600",
  applying: "bg-blue-100 text-blue-700",
  submitted: "bg-amber-100 text-amber-700",
  awarded: "bg-emerald-100 text-emerald-700",
  rejected: "bg-rose-100 text-rose-700",
};

interface ScholarshipCardProps {
  scholarship: ScholarshipDto;
  onEdit: () => void;
  onDelete: () => void;
}

export function ScholarshipCard({ scholarship, onEdit, onDelete }: ScholarshipCardProps) {
  const deadlineLabel = new Date(`${scholarship.deadline}T00:00:00`).toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
    year: "numeric",
  });

  return (
    <Card className="flex flex-col gap-2">
      <div className="flex items-start justify-between gap-2">
        <div>
          <p className="text-sm font-semibold text-gray-800">{scholarship.name}</p>
          {scholarship.amount != null && (
            <p className="text-xs text-gray-400">${scholarship.amount.toLocaleString()}</p>
          )}
        </div>
        <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${STATUS_BADGE_CLASSES[scholarship.status]}`}>
          {STATUS_LABELS[scholarship.status]}
        </span>
      </div>

      {scholarship.requirements.length > 0 && (
        <ul className="list-inside list-disc text-xs text-gray-500">
          {scholarship.requirements.map((req) => (
            <li key={req}>{req}</li>
          ))}
        </ul>
      )}

      <span className="text-xs text-gray-400">Deadline: {deadlineLabel}</span>

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

export { STATUS_LABELS };
