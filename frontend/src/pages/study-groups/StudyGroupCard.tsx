import { Link } from "react-router-dom";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import type { StudyGroupDto } from "../../features/studyGroups/types";

interface StudyGroupCardProps {
  group: StudyGroupDto;
}

export function StudyGroupCard({ group }: StudyGroupCardProps) {
  return (
    <Card className="flex flex-col gap-2">
      <div className="flex items-start justify-between gap-2">
        <div>
          <p className="text-sm font-semibold text-gray-800">{group.name}</p>
          <p className="text-xs text-gray-400">{group.schoolName}</p>
        </div>
        {group.isOwner && (
          <span className="rounded-full bg-indigo-100 px-2.5 py-0.5 text-xs font-medium text-indigo-700">Owner</span>
        )}
      </div>

      {group.subject && <p className="text-xs text-gray-500">{group.subject}</p>}

      <div className="flex items-center justify-between pt-1">
        <span className="text-xs text-gray-400">
          {group.memberCount} member{group.memberCount === 1 ? "" : "s"}
        </span>
        <Link to={`/study-groups/${group.id}`}>
          <Button variant="ghost" className="px-2 py-1 text-xs">
            View
          </Button>
        </Link>
      </div>
    </Card>
  );
}
