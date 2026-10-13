import { useEffect, useState, type FormEvent } from "react";
import { Button } from "../../components/ui/Button";
import { Input } from "../../components/ui/Input";
import { assignmentFormSchema, type AssignmentFormValues } from "../../features/assignments/schemas";

interface AssignmentFormProps {
  initialValues?: AssignmentFormValues;
  onSubmit: (values: AssignmentFormValues) => Promise<void>;
  onCancel: () => void;
  submitLabel: string;
}

const EMPTY_VALUES: AssignmentFormValues = {
  subject: "",
  title: "",
  description: "",
  dueDate: new Date().toISOString().slice(0, 10),
  priority: "medium",
};

export function AssignmentForm({ initialValues, onSubmit, onCancel, submitLabel }: AssignmentFormProps) {
  const [values, setValues] = useState<AssignmentFormValues>(initialValues ?? EMPTY_VALUES);
  const [errors, setErrors] = useState<Partial<Record<keyof AssignmentFormValues, string>>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  useEffect(() => {
    setValues(initialValues ?? EMPTY_VALUES);
  }, [initialValues]);

  const handleChange = (field: keyof AssignmentFormValues) => (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>,
  ) => {
    setValues((prev) => ({ ...prev, [field]: e.target.value }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setFormError(null);

    const parsed = assignmentFormSchema.safeParse(values);
    if (!parsed.success) {
      const fieldErrors: Partial<Record<keyof AssignmentFormValues, string>> = {};
      for (const issue of parsed.error.issues) {
        const field = issue.path[0] as keyof AssignmentFormValues;
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
      setFormError("Something went wrong saving this assignment. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form className="flex flex-col gap-3" onSubmit={handleSubmit} noValidate>
      <div className="grid grid-cols-2 gap-3">
        <Input label="Subject" value={values.subject} onChange={handleChange("subject")} error={errors.subject} />
        <label className="flex flex-col gap-1 text-sm font-medium text-gray-700">
          Priority
          <select
            className="rounded-xl border border-gray-300 px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-indigo-500"
            value={values.priority}
            onChange={handleChange("priority")}
          >
            <option value="low">Low</option>
            <option value="medium">Medium</option>
            <option value="high">High</option>
          </select>
        </label>
      </div>

      <Input label="Title" value={values.title} onChange={handleChange("title")} error={errors.title} />

      <label className="flex flex-col gap-1 text-sm font-medium text-gray-700">
        Notes (optional)
        <textarea
          className="rounded-xl border border-gray-300 px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-indigo-500"
          rows={2}
          value={values.description ?? ""}
          onChange={handleChange("description")}
        />
        {errors.description && <span className="text-xs text-red-500">{errors.description}</span>}
      </label>

      <Input
        label="Due date"
        type="date"
        value={values.dueDate}
        onChange={handleChange("dueDate")}
        error={errors.dueDate}
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
