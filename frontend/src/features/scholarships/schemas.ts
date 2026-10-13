import { z } from "zod";

// Mirrors StudentStudyHub.Application.Scholarships.CreateScholarshipRequestValidator /
// UpdateScholarshipRequestValidator on the backend.
export const scholarshipFormSchema = z.object({
  name: z.string().min(1, "Name is required").max(200),
  amount: z.string().optional().or(z.literal("")),
  deadline: z.string().min(1, "Deadline is required"),
  status: z.enum(["researching", "applying", "submitted", "awarded", "rejected"]),
  requirementsText: z.string().optional().or(z.literal("")),
});

export type ScholarshipFormValues = z.infer<typeof scholarshipFormSchema>;
