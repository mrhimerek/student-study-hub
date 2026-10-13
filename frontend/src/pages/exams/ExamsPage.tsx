import { useState } from "react";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import { useCreateExam, useDeleteExam, useExams, useUpdateExam } from "../../features/exams/hooks";
import type { ExamDto } from "../../features/exams/types";
import type { ExamFormValues } from "../../features/exams/schemas";
import { ExamCard } from "./ExamCard";
import { ExamForm } from "./ExamForm";

function toExamDateIso(dateOnly: string): string {
  // <input type="date"> gives "yyyy-MM-dd"; treat the exam time as end-of-day local time.
  return new Date(`${dateOnly}T23:59:00`).toISOString();
}

function toFormValues(exam: ExamDto): ExamFormValues {
  return {
    subject: exam.subject,
    title: exam.title,
    notes: exam.notes ?? "",
    examDate: exam.examDate.slice(0, 10),
    type: exam.type,
  };
}

export function ExamsPage() {
  const [isAdding, setIsAdding] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);

  const { data: exams, isLoading, isError } = useExams();
  const createMutation = useCreateExam();
  const updateMutation = useUpdateExam();
  const deleteMutation = useDeleteExam();

  const handleCreate = async (values: ExamFormValues) => {
    await createMutation.mutateAsync({
      subject: values.subject,
      title: values.title,
      notes: values.notes || null,
      examDate: toExamDateIso(values.examDate),
      type: values.type,
    });
    setIsAdding(false);
  };

  const handleUpdate = async (exam: ExamDto, values: ExamFormValues) => {
    await updateMutation.mutateAsync({
      id: exam.id,
      payload: {
        subject: values.subject,
        title: values.title,
        notes: values.notes || null,
        examDate: toExamDateIso(values.examDate),
        type: values.type,
      },
    });
    setEditingId(null);
  };

  const handleDelete = (exam: ExamDto) => {
    if (window.confirm(`Delete "${exam.title}"?`)) {
      deleteMutation.mutate(exam.id);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-semibold text-gray-800">Exams</h1>
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
          <ExamForm submitLabel="Add exam" onSubmit={handleCreate} onCancel={() => setIsAdding(false)} />
        </Card>
      )}

      {isLoading && <p className="text-sm text-gray-400">Loading exams…</p>}
      {isError && <p className="text-sm text-red-500">Couldn&apos;t load exams. Please try again.</p>}

      {!isLoading && exams?.length === 0 && (
        <p className="text-sm text-gray-400">No exams here yet. Tap "+ Add" to create one.</p>
      )}

      <div className="space-y-2">
        {exams?.map((exam) =>
          editingId === exam.id ? (
            <Card key={exam.id}>
              <ExamForm
                initialValues={toFormValues(exam)}
                submitLabel="Save changes"
                onSubmit={(values) => handleUpdate(exam, values)}
                onCancel={() => setEditingId(null)}
              />
            </Card>
          ) : (
            <ExamCard
              key={exam.id}
              exam={exam}
              onEdit={() => {
                setEditingId(exam.id);
                setIsAdding(false);
              }}
              onDelete={() => handleDelete(exam)}
            />
          ),
        )}
      </div>
    </div>
  );
}
