import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import type { GroupNoteDto } from "../../features/studyGroups/types";

interface GroupNoteCardProps {
  note: GroupNoteDto;
  canDelete: boolean;
  onEdit: () => void;
  onDelete: () => void;
}

export function GroupNoteCard({ note, canDelete, onEdit, onDelete }: GroupNoteCardProps) {
  const createdLabel = new Date(note.createdAt).toLocaleDateString(undefined, { month: "short", day: "numeric" });

  return (
    <Card className="flex flex-col gap-2">
      <div className="flex items-start justify-between gap-2">
        <p className="text-sm font-semibold text-gray-800">{note.title}</p>
        <span className="text-xs text-gray-400">{createdLabel}</span>
      </div>

      {note.content && <p className="whitespace-pre-wrap text-xs text-gray-600">{note.content}</p>}

      <div className="flex items-center justify-between pt-1">
        <span className="text-xs text-gray-400">by {note.authorName}</span>
        {(note.isMine || canDelete) && (
          <div className="flex gap-2">
            {note.isMine && (
              <Button variant="ghost" className="px-2 py-1 text-xs" onClick={onEdit}>
                Edit
              </Button>
            )}
            <Button variant="ghost" className="px-2 py-1 text-xs text-rose-600 hover:bg-rose-50" onClick={onDelete}>
              Delete
            </Button>
          </div>
        )}
      </div>
    </Card>
  );
}
