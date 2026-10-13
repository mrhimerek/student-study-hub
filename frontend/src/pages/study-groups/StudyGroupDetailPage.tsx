import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import { Input } from "../../components/ui/Input";
import {
  useCreateGroupNote,
  useDeleteGroupNote,
  useDeleteStudyGroup,
  useGroupNotes,
  useLeaveStudyGroup,
  useStudyGroup,
  useUpdateGroupNote,
  useUpdateStudyGroup,
} from "../../features/studyGroups/hooks";
import { studyGroupEditFormSchema, type StudyGroupEditFormValues } from "../../features/studyGroups/schemas";
import type { GroupNoteFormValues } from "../../features/studyGroups/schemas";
import type { GroupNoteDto } from "../../features/studyGroups/types";
import { GroupNoteCard } from "./GroupNoteCard";
import { GroupNoteForm } from "./GroupNoteForm";

function EditStudyGroupForm({
  initialValues,
  onSubmit,
  onCancel,
}: {
  initialValues: StudyGroupEditFormValues;
  onSubmit: (values: StudyGroupEditFormValues) => Promise<void>;
  onCancel: () => void;
}) {
  const [values, setValues] = useState(initialValues);
  const [errors, setErrors] = useState<Partial<Record<keyof StudyGroupEditFormValues, string>>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const parsed = studyGroupEditFormSchema.safeParse(values);
    if (!parsed.success) {
      const fieldErrors: Partial<Record<keyof StudyGroupEditFormValues, string>> = {};
      for (const issue of parsed.error.issues) {
        fieldErrors[issue.path[0] as keyof StudyGroupEditFormValues] = issue.message;
      }
      setErrors(fieldErrors);
      return;
    }
    setErrors({});
    setIsSubmitting(true);
    try {
      await onSubmit(parsed.data);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form className="flex flex-col gap-3" onSubmit={handleSubmit} noValidate>
      <Input
        label="Group name"
        value={values.name}
        onChange={(e) => setValues((prev) => ({ ...prev, name: e.target.value }))}
        error={errors.name}
      />
      <Input
        label="Subject (optional)"
        value={values.subject ?? ""}
        onChange={(e) => setValues((prev) => ({ ...prev, subject: e.target.value }))}
        error={errors.subject}
      />
      <div className="flex gap-2">
        <Button type="submit" disabled={isSubmitting} className="flex-1">
          {isSubmitting ? "Saving…" : "Save changes"}
        </Button>
        <Button type="button" variant="secondary" onClick={onCancel} disabled={isSubmitting}>
          Cancel
        </Button>
      </div>
    </form>
  );
}

export function StudyGroupDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [isEditing, setIsEditing] = useState(false);
  const [isAddingNote, setIsAddingNote] = useState(false);
  const [editingNoteId, setEditingNoteId] = useState<string | null>(null);

  const { data: group, isLoading, isError } = useStudyGroup(id);
  const { data: notes } = useGroupNotes(id);
  const updateMutation = useUpdateStudyGroup();
  const deleteMutation = useDeleteStudyGroup();
  const leaveMutation = useLeaveStudyGroup();
  const createNoteMutation = useCreateGroupNote(id ?? "");
  const updateNoteMutation = useUpdateGroupNote(id ?? "");
  const deleteNoteMutation = useDeleteGroupNote(id ?? "");

  if (isLoading) {
    return <p className="text-sm text-gray-400">Loading study group…</p>;
  }

  if (isError || !group) {
    return <p className="text-sm text-red-500">Couldn&apos;t load this study group.</p>;
  }

  const handleDeleteGroup = async () => {
    if (window.confirm(`Delete "${group.name}"? This cannot be undone.`)) {
      await deleteMutation.mutateAsync(group.id);
      navigate("/study-groups");
    }
  };

  const handleLeaveGroup = async () => {
    if (window.confirm(`Leave "${group.name}"?`)) {
      await leaveMutation.mutateAsync(group.id);
      navigate("/study-groups");
    }
  };

  const handleCreateNote = async (values: GroupNoteFormValues) => {
    await createNoteMutation.mutateAsync({ title: values.title, content: values.content || null });
    setIsAddingNote(false);
  };

  const handleUpdateNote = async (note: GroupNoteDto, values: GroupNoteFormValues) => {
    await updateNoteMutation.mutateAsync({
      noteId: note.id,
      payload: { title: values.title, content: values.content || null },
    });
    setEditingNoteId(null);
  };

  const handleDeleteNote = (note: GroupNoteDto) => {
    if (window.confirm(`Delete note "${note.title}"?`)) {
      deleteNoteMutation.mutate(note.id);
    }
  };

  return (
    <div className="space-y-4">
      <button className="text-xs text-indigo-600" onClick={() => navigate("/study-groups")}>
        ← Back to Study Groups
      </button>

      <Card>
        {isEditing ? (
          <EditStudyGroupForm
            initialValues={{ name: group.name, subject: group.subject ?? "" }}
            onSubmit={async (values) => {
              await updateMutation.mutateAsync({
                id: group.id,
                payload: { name: values.name, subject: values.subject || null },
              });
              setIsEditing(false);
            }}
            onCancel={() => setIsEditing(false)}
          />
        ) : (
          <div className="flex flex-col gap-2">
            <div className="flex items-start justify-between">
              <div>
                <h1 className="text-lg font-semibold text-gray-800">{group.name}</h1>
                <p className="text-xs text-gray-400">{group.schoolName}</p>
              </div>
              {group.isOwner && (
                <span className="rounded-full bg-indigo-100 px-2.5 py-0.5 text-xs font-medium text-indigo-700">Owner</span>
              )}
            </div>
            {group.subject && <p className="text-xs text-gray-500">{group.subject}</p>}
            <p className="text-xs text-gray-400">
              Invite code: <span className="font-mono font-semibold">{group.inviteCode}</span>
            </p>
            <div className="flex gap-2 pt-1">
              {group.isOwner ? (
                <>
                  <Button variant="ghost" className="px-2 py-1 text-xs" onClick={() => setIsEditing(true)}>
                    Edit
                  </Button>
                  <Button
                    variant="ghost"
                    className="px-2 py-1 text-xs text-rose-600 hover:bg-rose-50"
                    onClick={handleDeleteGroup}
                  >
                    Delete group
                  </Button>
                </>
              ) : (
                <Button
                  variant="ghost"
                  className="px-2 py-1 text-xs text-rose-600 hover:bg-rose-50"
                  onClick={handleLeaveGroup}
                >
                  Leave group
                </Button>
              )}
            </div>
          </div>
        )}
      </Card>

      <Card>
        <h2 className="mb-2 text-sm font-semibold text-gray-800">Members ({group.members.length})</h2>
        <ul className="space-y-1">
          {group.members.map((member) => (
            <li key={member.userId} className="flex items-center justify-between text-xs text-gray-600">
              <span>
                {member.firstName} {member.lastName}
              </span>
              {member.role === "owner" && <span className="text-indigo-600">Owner</span>}
            </li>
          ))}
        </ul>
      </Card>

      <div className="flex items-center justify-between">
        <h2 className="text-sm font-semibold text-gray-800">Shared Notes</h2>
        <Button
          className="px-3 py-1.5 text-xs"
          onClick={() => {
            setIsAddingNote((prev) => !prev);
            setEditingNoteId(null);
          }}
        >
          {isAddingNote ? "Close" : "+ Add note"}
        </Button>
      </div>

      {isAddingNote && (
        <Card>
          <GroupNoteForm submitLabel="Add note" onSubmit={handleCreateNote} onCancel={() => setIsAddingNote(false)} />
        </Card>
      )}

      {!notes?.length && !isAddingNote && <p className="text-sm text-gray-400">No notes yet. Add the first one!</p>}

      <div className="space-y-2">
        {notes?.map((note) =>
          editingNoteId === note.id ? (
            <Card key={note.id}>
              <GroupNoteForm
                initialValues={{ title: note.title, content: note.content ?? "" }}
                submitLabel="Save changes"
                onSubmit={(values) => handleUpdateNote(note, values)}
                onCancel={() => setEditingNoteId(null)}
              />
            </Card>
          ) : (
            <GroupNoteCard
              key={note.id}
              note={note}
              canDelete={group.isOwner}
              onEdit={() => {
                setEditingNoteId(note.id);
                setIsAddingNote(false);
              }}
              onDelete={() => handleDeleteNote(note)}
            />
          ),
        )}
      </div>
    </div>
  );
}
