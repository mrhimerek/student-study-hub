import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import { StatusChip } from "../../components/shared/StatusChip";
import type { AssignmentDto, AssignmentStatus } from "../../features/assignments/types";

const STATUS_LABELS: Record<AssignmentStatus, string> = {
  notStarted: "Not started",
  inProgress: "In progress",
  done: "Done",
};

const STATUS_CYCLE: Record<AssignmentStatus, AssignmentStatus> = {
  notStarted: "inProgress",
  inProgress: "done",
  done: "notStarted",
};

const STATUS_BADGE_CLASSES: Record<AssignmentStatus, string> = {
  notStarted: "bg-gray-100 text-gray-600",
  inProgress: "bg-blue-100 text-blue-700",
  done: "bg-emerald-100 text-emerald-700",
};

interface AssignmentCardProps {
  assignment: AssignmentDto;
  onCycleStatus: () => void;
  onEdit: () => void;
  onDelete: () => void;
  isUpdating?: boolean;
}

export function AssignmentCard({ assignment, onCycleStatus, onEdit, onDelete, isUpdating }: AssignmentCardProps) {
  const dueDate = new Date(assignment.dueDate);
  const dueLabel = dueDate.toLocaleDateString(undefined, { month: "short", day: "numeric" });

  return (
    <Card className="flex flex-col gap-2">
      <div className="flex items-start justify-between gap-2">
        <div>
          <p className="text-xs font-medium text-gray-400">{assignment.subject}</p>
          <p className="text-sm font-semibold text-gray-800">{assignment.title}</p>
        </div>
        <StatusChip priority={assignment.priority} />
      </div>

      {assignment.description && <p className="text-xs text-gray-500">{assignment.description}</p>}

      <div className="flex items-center justify-between">
        <span className="text-xs text-gray-400">Due {dueLabel}</span>
        <button
          type="button"
          onClick={onCycleStatus}
          disabled={isUpdating}
          className={`rounded-full px-2.5 py-0.5 text-xs font-medium disabled:opacity-50 ${STATUS_BADGE_CLASSES[assignment.status]}`}
          title="Tap to advance status"
        >
          {STATUS_LABELS[assignment.status]}
        </button>
      </div>

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

export { STATUS_CYCLE, STATUS_LABELS };
