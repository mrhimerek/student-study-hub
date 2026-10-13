import { z } from "zod";

// Mirrors StudentStudyHub.Application.Exams.CreateExamRequestValidator /
// UpdateExamRequestValidator on the backend.
export const examFormSchema = z.object({
  subject: z.string().min(1, "Subject is required").max(100),
  title: z.string().min(1, "Title is required").max(200),
  notes: z.string().max(2000).optional().or(z.literal("")),
  examDate: z.string().min(1, "Exam date is required"),
  type: z.enum(["quiz", "exam", "final"]),
});

export type ExamFormValues = z.infer<typeof examFormSchema>;
