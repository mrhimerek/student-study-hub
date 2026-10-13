import { useEffect, useState, type FormEvent } from "react";
import { Button } from "../../components/ui/Button";
import { Input } from "../../components/ui/Input";
import {
  collegeApplicationFormSchema,
  type CollegeApplicationFormValues,
} from "../../features/collegeApplications/schemas";

interface CollegeApplicationFormProps {
  initialValues?: CollegeApplicationFormValues;
  onSubmit: (values: CollegeApplicationFormValues) => Promise<void>;
  onCancel: () => void;
  submitLabel: string;
}

const EMPTY_VALUES: CollegeApplicationFormValues = {
  collegeName: "",
  applicationType: "regularDecision",
  deadline: new Date().toISOString().slice(0, 10),
  status: "notStarted",
  requirementsText: "",
};

export function CollegeApplicationForm({ initialValues, onSubmit, onCancel, submitLabel }: CollegeApplicationFormProps) {
  const [values, setValues] = useState<CollegeApplicationFormValues>(initialValues ?? EMPTY_VALUES);
  const [errors, setErrors] = useState<Partial<Record<keyof CollegeApplicationFormValues, string>>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  useEffect(() => {
    setValues(initialValues ?? EMPTY_VALUES);
  }, [initialValues]);

  const handleChange = (field: keyof CollegeApplicationFormValues) => (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>,
  ) => {
    setValues((prev) => ({ ...prev, [field]: e.target.value }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setFormError(null);

    const parsed = collegeApplicationFormSchema.safeParse(values);
    if (!parsed.success) {
      const fieldErrors: Partial<Record<keyof CollegeApplicationFormValues, string>> = {};
      for (const issue of parsed.error.issues) {
        const field = issue.path[0] as keyof CollegeApplicationFormValues;
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
      setFormError("Something went wrong saving this application. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form className="flex flex-col gap-3" onSubmit={handleSubmit} noValidate>
      <Input label="College name" value={values.collegeName} onChange={handleChange("collegeName")} error={errors.collegeName} />

      <div className="grid grid-cols-2 gap-3">
        <label className="flex flex-col gap-1 text-sm font-medium text-gray-700">
          Application type
          <select
            className="rounded-xl border border-gray-300 px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-indigo-500"
            value={values.applicationType}
            onChange={handleChange("applicationType")}
          >
            <option value="earlyDecision">Early Decision</option>
            <option value="earlyAction">Early Action</option>
            <option value="regularDecision">Regular Decision</option>
            <option value="rolling">Rolling</option>
          </select>
        </label>
        <label className="flex flex-col gap-1 text-sm font-medium text-gray-700">
          Status
          <select
            className="rounded-xl border border-gray-300 px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-indigo-500"
            value={values.status}
            onChange={handleChange("status")}
          >
            <option value="notStarted">Not Started</option>
            <option value="inProgress">In Progress</option>
            <option value="submitted">Submitted</option>
            <option value="accepted">Accepted</option>
            <option value="rejected">Rejected</option>
            <option value="waitlisted">Waitlisted</option>
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
