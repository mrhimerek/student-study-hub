import { useState } from "react";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import {
  useCreateCollegeApplication,
  useDeleteCollegeApplication,
  useCollegeApplications,
  useUpdateCollegeApplication,
} from "../../features/collegeApplications/hooks";
import type { CollegeApplicationDto } from "../../features/collegeApplications/types";
import type { CollegeApplicationFormValues } from "../../features/collegeApplications/schemas";
import { CollegeApplicationCard } from "./CollegeApplicationCard";
import { CollegeApplicationForm } from "./CollegeApplicationForm";

function requirementsFromText(text: string): string[] | null {
  const lines = text
    .split("\n")
    .map((line) => line.trim())
    .filter((line) => line.length > 0);
  return lines.length === 0 ? null : lines;
}

function toFormValues(application: CollegeApplicationDto): CollegeApplicationFormValues {
  return {
    collegeName: application.collegeName,
    applicationType: application.applicationType,
    deadline: application.deadline.slice(0, 10),
    status: application.status,
    requirementsText: application.requirements.join("\n"),
  };
}

export function CollegeApplicationsPage() {
  const [isAdding, setIsAdding] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);

  const { data: applications, isLoading, isError } = useCollegeApplications();
  const createMutation = useCreateCollegeApplication();
  const updateMutation = useUpdateCollegeApplication();
  const deleteMutation = useDeleteCollegeApplication();

  const handleCreate = async (values: CollegeApplicationFormValues) => {
    await createMutation.mutateAsync({
      collegeName: values.collegeName,
      applicationType: values.applicationType,
      deadline: values.deadline,
      status: values.status,
      requirements: requirementsFromText(values.requirementsText ?? ""),
    });
    setIsAdding(false);
  };

  const handleUpdate = async (application: CollegeApplicationDto, values: CollegeApplicationFormValues) => {
    await updateMutation.mutateAsync({
      id: application.id,
      payload: {
        collegeName: values.collegeName,
        applicationType: values.applicationType,
        deadline: values.deadline,
        status: values.status,
        requirements: requirementsFromText(values.requirementsText ?? ""),
      },
    });
    setEditingId(null);
  };

  const handleDelete = (application: CollegeApplicationDto) => {
    if (window.confirm(`Delete "${application.collegeName}"?`)) {
      deleteMutation.mutate(application.id);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-semibold text-gray-800">College Applications</h1>
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
          <CollegeApplicationForm submitLabel="Add application" onSubmit={handleCreate} onCancel={() => setIsAdding(false)} />
        </Card>
      )}

      {isLoading && <p className="text-sm text-gray-400">Loading college applications…</p>}
      {isError && <p className="text-sm text-red-500">Couldn&apos;t load college applications. Please try again.</p>}

      {!isLoading && applications?.length === 0 && (
        <p className="text-sm text-gray-400">No college applications here yet. Tap "+ Add" to create one.</p>
      )}

      <div className="space-y-2">
        {applications?.map((application) =>
          editingId === application.id ? (
            <Card key={application.id}>
              <CollegeApplicationForm
                initialValues={toFormValues(application)}
                submitLabel="Save changes"
                onSubmit={(values) => handleUpdate(application, values)}
                onCancel={() => setEditingId(null)}
              />
            </Card>
          ) : (
            <CollegeApplicationCard
              key={application.id}
              application={application}
              onEdit={() => {
                setEditingId(application.id);
                setIsAdding(false);
              }}
              onDelete={() => handleDelete(application)}
            />
          ),
        )}
      </div>
    </div>
  );
}
