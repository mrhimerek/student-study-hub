import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import type { CourseDto } from "../../features/courses/types";

interface CourseCardProps {
  course: CourseDto;
  onEdit: () => void;
  onDelete: () => void;
}

export function CourseCard({ course, onEdit, onDelete }: CourseCardProps) {
  return (
    <Card className="flex flex-col gap-2">
      <div className="flex items-start justify-between gap-2">
        <div>
          <p className="text-xs font-medium text-gray-400">{course.term}</p>
          <p className="text-sm font-semibold text-gray-800">{course.name}</p>
        </div>
        {course.isHonorsOrAp && (
          <span className="rounded-full bg-indigo-100 px-2.5 py-0.5 text-xs font-medium text-indigo-700">
            Honors/AP
          </span>
        )}
      </div>

      <div className="flex items-center justify-between text-xs text-gray-500">
        <span>{course.creditHours} credit{course.creditHours === 1 ? "" : "s"}</span>
        <span className="rounded-full bg-gray-100 px-2.5 py-0.5 font-semibold text-gray-700">
          {course.gradeValue}
        </span>
      </div>

      <div className="flex gap-2 pt-1">
        <Button variant="ghost" className="flex-1 px-2 py-1 text-xs" onClick={onEdit}>
          Edit
        </Button>
        <Button variant="ghost" className="flex-1 px-2 py-1 text-xs text-rose-600 hover:bg-rose-50" onClick={onDelete}>
          Delete
        </Button>
      </div>
    </Card>
  );
}
