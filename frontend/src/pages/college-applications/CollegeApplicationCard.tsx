import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import type {
  ApplicationType,
  CollegeApplicationDto,
  CollegeApplicationStatus,
} from "../../features/collegeApplications/types";

const TYPE_LABELS: Record<ApplicationType, string> = {
  earlyDecision: "Early Decision",
  earlyAction: "Early Action",
  regularDecision: "Regular Decision",
  rolling: "Rolling",
};

const STATUS_LABELS: Record<CollegeApplicationStatus, string> = {
  notStarted: "Not Started",
  inProgress: "In Progress",
  submitted: "Submitted",
  accepted: "Accepted",
  rejected: "Rejected",
  waitlisted: "Waitlisted",
};

const STATUS_BADGE_CLASSES: Record<CollegeApplicationStatus, string> = {
  notStarted: "bg-gray-100 text-gray-600",
  inProgress: "bg-blue-100 text-blue-700",
  submitted: "bg-amber-100 text-amber-700",
  accepted: "bg-emerald-100 text-emerald-700",
  rejected: "bg-rose-100 text-rose-700",
  waitlisted: "bg-purple-100 text-purple-700",
};

interface CollegeApplicationCardProps {
  application: CollegeApplicationDto;
  onEdit: () => void;
  onDelete: () => void;
}

export function CollegeApplicationCard({ application, onEdit, onDelete }: CollegeApplicationCardProps) {
  const deadlineLabel = new Date(`${application.deadline}T00:00:00`).toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
    year: "numeric",
  });

  return (
    <Card className="flex flex-col gap-2">
      <div className="flex items-start justify-between gap-2">
        <div>
          <p className="text-sm font-semibold text-gray-800">{application.collegeName}</p>
          <p className="text-xs text-gray-400">{TYPE_LABELS[application.applicationType]}</p>
        </div>
        <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${STATUS_BADGE_CLASSES[application.status]}`}>
          {STATUS_LABELS[application.status]}
        </span>
      </div>

      {application.requirements.length > 0 && (
        <ul className="list-inside list-disc text-xs text-gray-500">
          {application.requirements.map((req) => (
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

export { STATUS_LABELS, TYPE_LABELS };
