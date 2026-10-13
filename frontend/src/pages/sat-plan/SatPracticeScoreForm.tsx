import { useState, type FormEvent } from "react";
import { Button } from "../../components/ui/Button";
import { Input } from "../../components/ui/Input";
import { satPracticeScoreFormSchema, type SatPracticeScoreFormValues } from "../../features/satPlan/schemas";

interface SatPracticeScoreFormProps {
  onSubmit: (values: SatPracticeScoreFormValues) => Promise<void>;
  onCancel: () => void;
}

const EMPTY_VALUES: SatPracticeScoreFormValues = {
  testDate: new Date().toISOString().slice(0, 10),
  readingWritingScore: "600",
  mathScore: "600",
};

export function SatPracticeScoreForm({ onSubmit, onCancel }: SatPracticeScoreFormProps) {
  const [values, setValues] = useState<SatPracticeScoreFormValues>(EMPTY_VALUES);
  const [errors, setErrors] = useState<Partial<Record<keyof SatPracticeScoreFormValues, string>>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const handleChange = (field: keyof SatPracticeScoreFormValues) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setValues((prev) => ({ ...prev, [field]: e.target.value }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setFormError(null);

    const parsed = satPracticeScoreFormSchema.safeParse(values);
    if (!parsed.success) {
      const fieldErrors: Partial<Record<keyof SatPracticeScoreFormValues, string>> = {};
      for (const issue of parsed.error.issues) {
        const field = issue.path[0] as keyof SatPracticeScoreFormValues;
        fieldErrors[field] = issue.message;
      }
      setErrors(fieldErrors);
      return;
    }

    setErrors({});
    setIsSubmitting(true);
    try {
      await onSubmit(parsed.data);
      setValues(EMPTY_VALUES);
    } catch {
      setFormError("Something went wrong logging this score. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form className="flex flex-col gap-3" onSubmit={handleSubmit} noValidate>
      <Input
        label="Test date"
        type="date"
        value={values.testDate}
        onChange={handleChange("testDate")}
        error={errors.testDate}
      />
      <div className="grid grid-cols-2 gap-3">
        <Input
          label="Reading & Writing"
          type="number"
          min="200"
          max="800"
          value={values.readingWritingScore}
          onChange={handleChange("readingWritingScore")}
          error={errors.readingWritingScore}
        />
        <Input
          label="Math"
          type="number"
          min="200"
          max="800"
          value={values.mathScore}
          onChange={handleChange("mathScore")}
          error={errors.mathScore}
        />
      </div>

      {formError && <p className="text-sm text-red-500">{formError}</p>}

      <div className="flex gap-2">
        <Button type="submit" disabled={isSubmitting} className="flex-1">
          {isSubmitting ? "Saving…" : "Log score"}
        </Button>
        <Button type="button" variant="secondary" onClick={onCancel} disabled={isSubmitting}>
          Cancel
        </Button>
      </div>
    </form>
  );
}
