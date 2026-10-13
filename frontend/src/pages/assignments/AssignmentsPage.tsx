import { useState } from "react";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import {
  useAssignments,
  useCreateAssignment,
  useDeleteAssignment,
  useUpdateAssignment,
} from "../../features/assignments/hooks";
import type { AssignmentDto, AssignmentStatus } from "../../features/assignments/types";
import type { AssignmentFormValues } from "../../features/assignments/schemas";
import { AssignmentCard, STATUS_CYCLE } from "./AssignmentCard";
import { AssignmentForm } from "./AssignmentForm";

type FilterTab = "all" | AssignmentStatus;

const FILTER_TABS: { value: FilterTab; label: string }[] = [
  { value: "all", label: "All" },
  { value: "notStarted", label: "Not started" },
  { value: "inProgress", label: "In progress" },
  { value: "done", label: "Done" },
];

function toDueDateIso(dateOnly: string): string {
  // <input type="date"> gives "yyyy-MM-dd"; treat the due time as end-of-day local time.
  return new Date(`${dateOnly}T23:59:00`).toISOString();
}

function toFormValues(assignment: AssignmentDto): AssignmentFormValues {
  return {
    subject: assignment.subject,
    title: assignment.title,
    description: assignment.description ?? "",
    dueDate: assignment.dueDate.slice(0, 10),
    priority: assignment.priority,
  };
}

export function AssignmentsPage() {
  const [filter, setFilter] = useState<FilterTab>("all");
  const [isAdding, setIsAdding] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);

  const { data: assignments, isLoading, isError } = useAssignments(filter === "all" ? undefined : filter);
  const createMutation = useCreateAssignment();
  const updateMutation = useUpdateAssignment();
  const deleteMutation = useDeleteAssignment();

  const handleCreate = async (values: AssignmentFormValues) => {
    await createMutation.mutateAsync({
      subject: values.subject,
      title: values.title,
      description: values.description || null,
      dueDate: toDueDateIso(values.dueDate),
      priority: values.priority,
    });
    setIsAdding(false);
  };

  const handleUpdate = async (assignment: AssignmentDto, values: AssignmentFormValues) => {
    await updateMutation.mutateAsync({
      id: assignment.id,
      payload: {
        subject: values.subject,
        title: values.title,
        description: values.description || null,
        dueDate: toDueDateIso(values.dueDate),
        priority: values.priority,
        status: assignment.status,
      },
    });
    setEditingId(null);
  };

  const handleCycleStatus = (assignment: AssignmentDto) => {
    updateMutation.mutate({
      id: assignment.id,
      payload: {
        subject: assignment.subject,
        title: assignment.title,
        description: assignment.description,
        dueDate: assignment.dueDate,
        priority: assignment.priority,
        status: STATUS_CYCLE[assignment.status],
      },
    });
  };

  const handleDelete = (assignment: AssignmentDto) => {
    if (window.confirm(`Delete "${assignment.title}"?`)) {
      deleteMutation.mutate(assignment.id);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-semibold text-gray-800">Assignments</h1>
        <Button
          onClick={() => {
            setIsAdding((prev) => !prev);
            setEditingId(null);
          }}
        >
          {isAdding ? "Close" : "+ Add"}
        </Button>
      </div>

      <div className="flex gap-2 overflow-x-auto pb-1">
        {FILTER_TABS.map((tab) => (
          <button
            key={tab.value}
            type="button"
            onClick={() => setFilter(tab.value)}
            className={`whitespace-nowrap rounded-full px-3 py-1 text-xs font-medium ${
              filter === tab.value ? "bg-indigo-600 text-white" : "bg-gray-100 text-gray-600"
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {isAdding && (
        <Card>
          <AssignmentForm submitLabel="Add assignment" onSubmit={handleCreate} onCancel={() => setIsAdding(false)} />
        </Card>
      )}

      {isLoading && <p className="text-sm text-gray-400">Loading assignments…</p>}
      {isError && <p className="text-sm text-red-500">Couldn&apos;t load assignments. Please try again.</p>}

      {!isLoading && assignments?.length === 0 && (
        <p className="text-sm text-gray-400">No assignments here yet. Tap "+ Add" to create one.</p>
      )}

      <div className="space-y-2">
        {assignments?.map((assignment) =>
          editingId === assignment.id ? (
            <Card key={assignment.id}>
              <AssignmentForm
                initialValues={toFormValues(assignment)}
                submitLabel="Save changes"
                onSubmit={(values) => handleUpdate(assignment, values)}
                onCancel={() => setEditingId(null)}
              />
            </Card>
          ) : (
            <AssignmentCard
              key={assignment.id}
              assignment={assignment}
              isUpdating={updateMutation.isPending}
              onCycleStatus={() => handleCycleStatus(assignment)}
              onEdit={() => {
                setEditingId(assignment.id);
                setIsAdding(false);
              }}
              onDelete={() => handleDelete(assignment)}
            />
          ),
        )}
      </div>
    </div>
  );
}
