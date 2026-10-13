import { useState } from "react";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import { useCreateScholarship, useDeleteScholarship, useScholarships, useUpdateScholarship } from "../../features/scholarships/hooks";
import type { ScholarshipDto } from "../../features/scholarships/types";
import type { ScholarshipFormValues } from "../../features/scholarships/schemas";
import { ScholarshipCard } from "./ScholarshipCard";
import { ScholarshipForm } from "./ScholarshipForm";

function requirementsFromText(text: string): string[] | null {
  const lines = text
    .split("\n")
    .map((line) => line.trim())
    .filter((line) => line.length > 0);
  return lines.length === 0 ? null : lines;
}

function toFormValues(scholarship: ScholarshipDto): ScholarshipFormValues {
  return {
    name: scholarship.name,
    amount: scholarship.amount != null ? String(scholarship.amount) : "",
    deadline: scholarship.deadline.slice(0, 10),
    status: scholarship.status,
    requirementsText: scholarship.requirements.join("\n"),
  };
}

export function ScholarshipsPage() {
  const [isAdding, setIsAdding] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);

  const { data: scholarships, isLoading, isError } = useScholarships();
  const createMutation = useCreateScholarship();
  const updateMutation = useUpdateScholarship();
  const deleteMutation = useDeleteScholarship();

  const handleCreate = async (values: ScholarshipFormValues) => {
    await createMutation.mutateAsync({
      name: values.name,
      amount: values.amount ? Number(values.amount) : null,
      deadline: values.deadline,
      status: values.status,
      requirements: requirementsFromText(values.requirementsText ?? ""),
    });
    setIsAdding(false);
  };

  const handleUpdate = async (scholarship: ScholarshipDto, values: ScholarshipFormValues) => {
    await updateMutation.mutateAsync({
      id: scholarship.id,
      payload: {
        name: values.name,
        amount: values.amount ? Number(values.amount) : null,
        deadline: values.deadline,
        status: values.status,
        requirements: requirementsFromText(values.requirementsText ?? ""),
      },
    });
    setEditingId(null);
  };

  const handleDelete = (scholarship: ScholarshipDto) => {
    if (window.confirm(`Delete "${scholarship.name}"?`)) {
      deleteMutation.mutate(scholarship.id);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-semibold text-gray-800">Scholarships</h1>
        <Button
          onClick={() => {
            setIsAdding((prev) => !prev);
            setEditingId(null);
          }}
        >
          {isAdding ? "Close" : "+ Add"}
        </Button>
      </div>

      {isAdding && (
        <Card>
          <ScholarshipForm submitLabel="Add scholarship" onSubmit={handleCreate} onCancel={() => setIsAdding(false)} />
        </Card>
      )}

      {isLoading && <p className="text-sm text-gray-400">Loading scholarships…</p>}
      {isError && <p className="text-sm text-red-500">Couldn&apos;t load scholarships. Please try again.</p>}

      {!isLoading && scholarships?.length === 0 && (
        <p className="text-sm text-gray-400">No scholarships here yet. Tap "+ Add" to create one.</p>
      )}

      <div className="space-y-2">
        {scholarships?.map((scholarship) =>
          editingId === scholarship.id ? (
            <Card key={scholarship.id}>
              <ScholarshipForm
                initialValues={toFormValues(scholarship)}
                submitLabel="Save changes"
                onSubmit={(values) => handleUpdate(scholarship, values)}
                onCancel={() => setEditingId(null)}
              />
            </Card>
          ) : (
            <ScholarshipCard
              key={scholarship.id}
              scholarship={scholarship}
              onEdit={() => {
                setEditingId(scholarship.id);
                setIsAdding(false);
              }}
              onDelete={() => handleDelete(scholarship)}
            />
          ),
        )}
      </div>
    </div>
  );
}
