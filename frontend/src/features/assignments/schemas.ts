import { z } from "zod";

// Mirrors StudentStudyHub.Application.Assignments.CreateAssignmentRequestValidator /
// UpdateAssignmentRequestValidator on the backend.
export const assignmentFormSchema = z.object({
  subject: z.string().min(1, "Subject is required").max(100),
  title: z.string().min(1, "Title is required").max(200),
  description: z.string().max(2000).optional().or(z.literal("")),
  dueDate: z.string().min(1, "Due date is required"),
  priority: z.enum(["low", "medium", "high"]),
});

export type AssignmentFormValues = z.infer<typeof assignmentFormSchema>;
