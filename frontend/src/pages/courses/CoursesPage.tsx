import { useState } from "react";
import { Button } from "../../components/ui/Button";
import { Card } from "../../components/ui/Card";
import { useCourses, useCreateCourse, useDeleteCourse, useUpdateCourse } from "../../features/courses/hooks";
import type { CourseDto } from "../../features/courses/types";
import type { CourseFormValues } from "../../features/courses/schemas";
import { CourseCard } from "./CourseCard";
import { CourseForm } from "./CourseForm";

function toFormValues(course: CourseDto): CourseFormValues {
  return {
    name: course.name,
    creditHours: course.creditHours,
    gradeValue: course.gradeValue,
    isHonorsOrAp: course.isHonorsOrAp,
    term: course.term,
  };
}

export function CoursesPage() {
  const [isAdding, setIsAdding] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);

  const { data: courses, isLoading, isError } = useCourses();
  const createMutation = useCreateCourse();
  const updateMutation = useUpdateCourse();
  const deleteMutation = useDeleteCourse();

  const handleCreate = async (values: CourseFormValues) => {
    await createMutation.mutateAsync(values);
    setIsAdding(false);
  };

  const handleUpdate = async (course: CourseDto, values: CourseFormValues) => {
    await updateMutation.mutateAsync({ id: course.id, payload: values });
    setEditingId(null);
  };

  const handleDelete = (course: CourseDto) => {
    if (window.confirm(`Delete "${course.name}"?`)) {
      deleteMutation.mutate(course.id);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-semibold text-gray-800">Courses</h1>
        <Button
          onClick={() => {
            setIsAdding((prev) => !prev);
            setEditingId(null);
          }}
        >
          {isAdding ? "Close" : "+ Add"}
        </Button>
      </div>

      <p className="text-xs text-gray-400">
        Add your courses and grades here to power the GPA calculator on your dashboard.
      </p>

      {isAdding && (
        <Card>
          <CourseForm submitLabel="Add course" onSubmit={handleCreate} onCancel={() => setIsAdding(false)} />
        </Card>
      )}

      {isLoading && <p className="text-sm text-gray-400">Loading courses…</p>}
      {isError && <p className="text-sm text-red-500">Couldn&apos;t load courses. Please try again.</p>}

      {!isLoading && courses?.length === 0 && (
        <p className="text-sm text-gray-400">No courses yet. Tap "+ Add" to create one.</p>
      )}

      <div className="space-y-2">
        {courses?.map((course) =>
          editingId === course.id ? (
            <Card key={course.id}>
              <CourseForm
                initialValues={toFormValues(course)}
                submitLabel="Save changes"
                onSubmit={(values) => handleUpdate(course, values)}
                onCancel={() => setEditingId(null)}
              />
            </Card>
          ) : (
            <CourseCard
              key={course.id}
              course={course}
              onEdit={() => {
                setEditingId(course.id);
                setIsAdding(false);
              }}
              onDelete={() => handleDelete(course)}
            />
          ),
        )}
      </div>
    </div>
  );
}
