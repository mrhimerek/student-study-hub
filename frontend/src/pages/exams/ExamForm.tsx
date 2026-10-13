import { useEffect, useState, type FormEvent } from "react";
import { Button } from "../../components/ui/Button";
import { Input } from "../../components/ui/Input";
import { examFormSchema, type ExamFormValues } from "../../features/exams/schemas";

interface ExamFormProps {
  initialValues?: ExamFormValues;
  onSubmit: (values: ExamFormValues) => Promise<void>;
  onCancel: () => void;
  submitLabel: string;
}

const EMPTY_VALUES: ExamFormValues = {
  subject: "",
  title: "",
  notes: "",
  examDate: new Date().toISOString().slice(0, 10),
  type: "exam",
};

export function ExamForm({ initialValues, onSubmit, onCancel, submitLabel }: ExamFormProps) {
  const [values, setValues] = useState<ExamFormValues>(initialValues ?? EMPTY_VALUES);
  const [errors, setErrors] = useState<Partial<Record<keyof ExamFormValues, string>>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  useEffect(() => {
    setValues(initialValues ?? EMPTY_VALUES);
  }, [initialValues]);

  const handleChange = (field: keyof ExamFormValues) => (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>,
  ) => {
    setValues((prev) => ({ ...prev, [field]: e.target.value }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setFormError(null);

    const parsed = examFormSchema.safeParse(values);
    if (!parsed.success) {
      const fieldErrors: Partial<Record<keyof ExamFormValues, string>> = {};
      for (const issue of parsed.error.issues) {
        const field = issue.path[0] as keyof ExamFormValues;
        fieldErrors[field] = issue.message;
      }
      setErrors(fieldErrors);
      return;
    }

    setErrors({});
    setIsSubmitting(true);
    try {
      await onSubmit(parsed.data);
    } catch {
      setFormError("Something went wrong saving this exam. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form className="flex flex-col gap-3" onSubmit={handleSubmit} noValidate>
      <div className="grid grid-cols-2 gap-3">
        <Input label="Subject" value={values.subject} onChange={handleChange("subject")} error={errors.subject} />
        <label className="flex flex-col gap-1 text-sm font-medium text-gray-700">
          Type
          <select
            className="rounded-xl border border-gray-300 px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-indigo-500"
            value={values.type}
            onChange={handleChange("type")}
          >
            <option value="quiz">Quiz</option>
            <option value="exam">Exam</option>
            <option value="final">Final</option>
          </select>
        </label>
      </div>

      <Input label="Title" value={values.title} onChange={handleChange("title")} error={errors.title} />

      <label className="flex flex-col gap-1 text-sm font-medium text-gray-700">
        Notes (optional)
        <textarea
          className="rounded-xl border border-gray-300 px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-indigo-500"
          rows={2}
          value={values.notes ?? ""}
          onChange={handleChange("notes")}
        />
        {errors.notes && <span className="text-xs text-red-500">{errors.notes}</span>}
      </label>

      <Input
        label="Exam date"
        type="date"
        value={values.examDate}
        onChange={handleChange("examDate")}
        error={errors.examDate}
      />

      {formError && <p className="text-sm text-red-500">{formError}</p>}

      <div className="flex gap-2">
        <Button type="submit" disabled={isSubmitting} className="flex-1">
          {isSubmitting ? "Saving…" : submitLabel}
        </Button>
        <Button type="button" variant="secondary" onClick={onCancel} disabled={isSubmitting}>
          Cancel
        </Button>
      </div>
    </form>
  );
}
