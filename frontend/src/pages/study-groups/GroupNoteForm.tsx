import { useState, type FormEvent } from "react";
import { Button } from "../../components/ui/Button";
import { Input } from "../../components/ui/Input";
import { groupNoteFormSchema, type GroupNoteFormValues } from "../../features/studyGroups/schemas";

interface GroupNoteFormProps {
  initialValues?: GroupNoteFormValues;
  onSubmit: (values: GroupNoteFormValues) => Promise<void>;
  onCancel: () => void;
  submitLabel: string;
}

const EMPTY_VALUES: GroupNoteFormValues = { title: "", content: "" };

export function GroupNoteForm({ initialValues, onSubmit, onCancel, submitLabel }: GroupNoteFormProps) {
  const [values, setValues] = useState<GroupNoteFormValues>(initialValues ?? EMPTY_VALUES);
  const [errors, setErrors] = useState<Partial<Record<keyof GroupNoteFormValues, string>>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const handleChange = (field: keyof GroupNoteFormValues) => (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>,
  ) => {
    setValues((prev) => ({ ...prev, [field]: e.target.value }));
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setFormError(null);

    const parsed = groupNoteFormSchema.safeParse(values);
    if (!parsed.success) {
      const fieldErrors: Partial<Record<keyof GroupNoteFormValues, string>> = {};
      for (const issue of parsed.error.issues) {
        const field = issue.path[0] as keyof GroupNoteFormValues;
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
      setFormError("Something went wrong saving this note. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form className="flex flex-col gap-3" onSubmit={handleSubmit} noValidate>
      <Input label="Title" value={values.title} onChange={handleChange("title")} error={errors.title} />

      <label className="flex flex-col gap-1 text-sm font-medium text-gray-700">
        Content (optional)
        <textarea
          className="rounded-xl border border-gray-300 px-3 py-2 text-sm outline-none focus:ring-2 focus:ring-indigo-500"
          rows={4}
          value={values.content ?? ""}
          onChange={handleChange("content")}
        />
        {errors.content && <span className="text-xs text-red-500">{errors.content}</span>}
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
