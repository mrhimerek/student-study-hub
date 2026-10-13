import { useState, type FormEvent } from "react";
import { Button } from "../../components/ui/Button";
import { Input } from "../../components/ui/Input";
import { joinStudyGroupFormSchema, type JoinStudyGroupFormValues } from "../../features/studyGroups/schemas";

interface JoinStudyGroupFormProps {
  onSubmit: (values: JoinStudyGroupFormValues) => Promise<void>;
  onCancel: () => void;
}

export function JoinStudyGroupForm({ onSubmit, onCancel }: JoinStudyGroupFormProps) {
  const [inviteCode, setInviteCode] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setFormError(null);

    const parsed = joinStudyGroupFormSchema.safeParse({ inviteCode });
    if (!parsed.success) {
      setError(parsed.error.issues[0]?.message ?? "Invalid invite code");
      return;
    }

    setError(null);
    setIsSubmitting(true);
    try {
      await onSubmit(parsed.data);
      setInviteCode("");
    } catch {
      setFormError("Couldn't join with that invite code. Please double-check it and try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form className="flex flex-col gap-3" onSubmit={handleSubmit} noValidate>
      <Input
        label="Invite code"
        value={inviteCode}
        onChange={(e) => setInviteCode(e.target.value.toUpperCase())}
        error={error ?? undefined}
        maxLength={8}
        placeholder="e.g. AB3DEFGH"
      />

      {formError && <p className="text-sm text-red-500">{formError}</p>}

      <div className="flex gap-2">
        <Button type="submit" disabled={isSubmitting} className="flex-1">
          {isSubmitting ? "Joining…" : "Join group"}
        </Button>
        <Button type="button" variant="secondary" onClick={onCancel} disabled={isSubmitting}>
          Cancel
        </Button>
      </div>
    </form>
  );
}
