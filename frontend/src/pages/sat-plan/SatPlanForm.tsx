import { useEffect, useState, type FormEvent } from "react";
import { Button } from "../../components/ui/Button";
import { Input } from "../../components/ui/Input";
import { satPlanFormSchema, type SatPlanFormValues } from "../../features/satPlan/schemas";

interface SatPlanFormProps {
  initialValues?: SatPlanFormValues;
  onSubmit: (values: SatPlanFormValues) => Promise<void>;
  onCancel?: () => void;
  submitLabel: string;
}

const EMPTY_VALUES: SatPlanFormValues = {
  targetTestDate: new Date().toISOString().slice(0, 10),
  goalScore: "1400",
};

export function SatPlanForm({ initialValues, onSubmit, onCancel, submitLabel }: SatPlanFormProps) {
  const [values, setValues] = useState<SatPlanFormValues>(initialValues ?? EMPTY_VALUES);
  const [errors, setErrors] = useState<Partial<Record<keyof SatPlanFormValues, string>>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  useEffect(() => {
    setValues(initialValues ?? EMPTY_VALUES);
  }, [initialValues]);

  const handleChange = (field: keyof SatPlanFormValues) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setValues((prev) => ({ ...prev, [field]: e.target.value }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setFormError(null);

    const parsed = satPlanFormSchema.safeParse(values);
    if (!parsed.success) {
      const fieldErrors: Partial<Record<keyof SatPlanFormValues, string>> = {};
      for (const issue of parsed.error.issues) {
        const field = issue.path[0] as keyof SatPlanFormValues;
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
      setFormError("Something went wrong saving your SAT plan. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form className="flex flex-col gap-3" onSubmit={handleSubmit} noValidate>
      <Input
        label="Target test date"
        type="date"
        value={values.targetTestDate}
        onChange={handleChange("targetTestDate")}
        error={errors.targetTestDate}
      />
      <Input
        label="Goal score (400-1600)"
        type="number"
        min="400"
        max="1600"
        value={values.goalScore}
        onChange={handleChange("goalScore")}
        error={errors.goalScore}
      />

      {formError && <p className="text-sm text-red-500">{formError}</p>}

      <div className="flex gap-2">
        <Button type="submit" disabled={isSubmitting} className="flex-1">
          {isSubmitting ? "Saving…" : submitLabel}
        </Button>
        {onCancel && (
          <Button type="button" variant="secondary" onClick={onCancel} disabled={isSubmitting}>
            Cancel
          </Button>
        )}
      </div>
    </form>
  );
}
