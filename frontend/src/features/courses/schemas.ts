import { z } from "zod";

// Mirrors StudentStudyHub.Application.Courses.CreateCourseRequestValidator /
// UpdateCourseRequestValidator on the backend.
export const courseFormSchema = z.object({
  name: z.string().min(1, "Course name is required").max(150),
  creditHours: z.coerce.number().gt(0, "Must be greater than 0").lte(10, "Must be 10 or less"),
  gradeValue: z.string().min(1, "Grade is required").max(5),
  isHonorsOrAp: z.boolean(),
  term: z.string().min(1, "Term is required").max(50),
});

export type CourseFormValues = z.infer<typeof courseFormSchema>;
