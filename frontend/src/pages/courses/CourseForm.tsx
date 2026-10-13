import { useEffect, useState, type ChangeEvent, type FormEvent } from "react";
import { Button } from "../../components/ui/Button";
import { Input } from "../../components/ui/Input";
import { courseFormSchema, type CourseFormValues } from "../../features/courses/schemas";

interface CourseFormProps {
  initialValues?: CourseFormValues;
  onSubmit: (values: CourseFormValues) => Promise<void>;
  onCancel: () => void;
  submitLabel: string;
}

const EMPTY_VALUES: CourseFormValues = {
  name: "",
  creditHours: 1,
  gradeValue: "",
  isHonorsOrAp: false,
  term: "",
};

export function CourseForm({ initialValues, onSubmit, onCancel, submitLabel }: CourseFormProps) {
  const [values, setValues] = useState<CourseFormValues>(initialValues ?? EMPTY_VALUES);
  const [errors, setErrors] = useState<Partial<Record<keyof CourseFormValues, string>>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  useEffect(() => {
    setValues(initialValues ?? EMPTY_VALUES);
  }, [initialValues]);

  const handleChange = (field: keyof CourseFormValues) => (e: ChangeEvent<HTMLInputElement>) => {
    const value = field === "isHonorsOrAp" ? e.target.checked : e.target.value;
    setValues((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setFormError(null);

    const parsed = courseFormSchema.safeParse(values);
    if (!parsed.success) {
      const fieldErrors: Partial<Record<keyof CourseFormValues, string>> = {};
      for (const issue of parsed.error.issues) {
        const field = issue.path[0] as keyof CourseFormValues;
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
      setFormError("Something went wrong saving this course. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form className="flex flex-col gap-3" onSubmit={handleSubmit} noValidate>
      <Input label="Course name" value={values.name} onChange={handleChange("name")} error={errors.name} />

      <div className="grid grid-cols-2 gap-3">
        <Input label="Term" value={values.term} onChange={handleChange("term")} error={errors.term} />
        <Input label="Grade" value={values.gradeValue} onChange={handleChange("gradeValue")} error={errors.gradeValue} />
      </div>

      <div className="grid grid-cols-2 gap-3">
        <Input
          label="Credit hours"
          type="number"
          step="0.5"
          min="0.5"
          max="10"
          value={values.creditHours}
          onChange={handleChange("creditHours")}
          error={errors.creditHours}
        />
        <label className="flex items-center gap-2 pt-6 text-sm font-medium text-gray-700">
          <input type="checkbox" checked={values.isHonorsOrAp} onChange={handleChange("isHonorsOrAp")} />
          Honors/AP
        </label>
      </div>

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
