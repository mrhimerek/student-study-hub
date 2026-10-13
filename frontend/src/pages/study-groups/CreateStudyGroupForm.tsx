import { useState, type FormEvent } from "react";
import { Button } from "../../components/ui/Button";
import { Input } from "../../components/ui/Input";
import { studyGroupFormSchema, type StudyGroupFormValues } from "../../features/studyGroups/schemas";

interface CreateStudyGroupFormProps {
  onSubmit: (values: StudyGroupFormValues) => Promise<void>;
  onCancel: () => void;
}

const EMPTY_VALUES: StudyGroupFormValues = { name: "", subject: "", schoolName: "" };

export function CreateStudyGroupForm({ onSubmit, onCancel }: CreateStudyGroupFormProps) {
  const [values, setValues] = useState<StudyGroupFormValues>(EMPTY_VALUES);
  const [errors, setErrors] = useState<Partial<Record<keyof StudyGroupFormValues, string>>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const handleChange = (field: keyof StudyGroupFormValues) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setValues((prev) => ({ ...prev, [field]: e.target.value }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setFormError(null);

    const parsed = studyGroupFormSchema.safeParse(values);
    if (!parsed.success) {
      const fieldErrors: Partial<Record<keyof StudyGroupFormValues, string>> = {};
      for (const issue of parsed.error.issues) {
        const field = issue.path[0] as keyof StudyGroupFormValues;
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
      setFormError("Something went wrong creating this study group. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form className="flex flex-col gap-3" onSubmit={handleSubmit} noValidate>
      <Input label="Group name" value={values.name} onChange={handleChange("name")} error={errors.name} />
      <Input
        label="Subject (optional)"
        value={values.subject ?? ""}
        onChange={handleChange("subject")}
        error={errors.subject}
      />
      <Input
        label="School"
        value={values.schoolName}
        onChange={handleChange("schoolName")}
        error={errors.schoolName}
      />

      {formError && <p className="text-sm text-red-500">{formError}</p>}

      <div className="flex gap-2">
        <Button type="submit" disabled={isSubmitting} className="flex-1">
          {isSubmitting ? "Creating…" : "Create group"}
        </Button>
        <Button type="button" variant="secondary" onClick={onCancel} disabled={isSubmitting}>
          Cancel
        </Button>
      </div>
    </form>
  );
}
