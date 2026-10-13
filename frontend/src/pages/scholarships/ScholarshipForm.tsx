import { useEffect, useState, type FormEvent } from "react";
import { Button } from "../../components/ui/Button";
import { Input } from "../../components/ui/Input";
import { scholarshipFormSchema, type ScholarshipFormValues } from "../../features/scholarships/schemas";

interface ScholarshipFormProps {
  initialValues?: ScholarshipFormValues;
  onSubmit: (values: ScholarshipFormValues) => Promise<void>;
  onCancel: () => void;
  submitLabel: string;
}

const EMPTY_VALUES: ScholarshipFormValues = {
  name: "",
  amount: "",
  deadline: new Date().toISOString().slice(0, 10),
  status: "researching",
  requirementsText: "",
};

export function ScholarshipForm({ initialValues, onSubmit, onCancel, submitLabel }: ScholarshipFormProps) {
  const [values, setValues] = useState<ScholarshipFormValues>(initialValues ?? EMPTY_VALUES);
  const [errors, setErrors] = useState<Partial<Record<keyof ScholarshipFormValues, string>>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  useEffect(() => {
    setValues(initialValues ?? EMPTY_VALUES);
  }, [initialValues]);

  const handleChange = (field: keyof ScholarshipFormValues) => (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>,
  ) => {
    setValues((prev) => ({ ...prev, [field]: e.target.value }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setFormError(null);

    const parsed = scholarshipFormSchema.safeParse(values);
    if (!parsed.success) {
      const fieldErrors: Partial<Record<keyof ScholarshipFormValues, string>> = {};
      for (const issue of parsed.error.issues) {
        const field = issue.path[0] as keyof ScholarshipFormValues;
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
      setFormError("Something went wrong saving this scholarship. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form className="flex flex-col gap-3" onSubmit={handleSubmit} noValidate>
      <Input label="Name" value={values.name} onChange={handleChange("name")} error={errors.name} />

      <div className="grid grid-cols-2 gap-3">
        <Input
          label="Amount (optional)"
          type="number"
          min="0"
          step="0.01"
          value={values.amount ?? ""}
          onChange={handleChange("amount")}
          error={errors.amount}
        />
        <label className="flex flex-col gap-1 text-sm font-medium text-gray-700">
          Status
          <select
            className="rounded-xl border border-gray-300 px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-indigo-500"
            value={values.status}
            onChange={handleChange("status")}
          >
            <option value="researching">Researching</option>
            <option value="applying">Applying</option>
            <option value="submitted">Submitted</option>
            <option value="awarded">Awarded</option>
            <option value="rejected">Rejected</option>
          </select>
        </label>
      </div>

      <Input
        label="Deadline"
        type="date"
        value={values.deadline}
        onChange={handleChange("deadline")}
        error={errors.deadline}
      />

      <label className="flex flex-col gap-1 text-sm font-medium text-gray-700">
        Requirements (one per line, optional)
        <textarea
          className="rounded-xl border border-gray-300 px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-indigo-500"
          rows={3}
          value={values.requirementsText ?? ""}
          onChange={handleChange("requirementsText")}
        />
        {errors.requirementsText && <span className="text-xs text-red-500">{errors.requirementsText}</span>}
      </label>

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
