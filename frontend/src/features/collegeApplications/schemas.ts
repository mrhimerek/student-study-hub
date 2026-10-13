import { z } from "zod";

// Mirrors StudentStudyHub.Application.CollegeApplications.CreateCollegeApplicationRequestValidator /
// UpdateCollegeApplicationRequestValidator on the backend.
export const collegeApplicationFormSchema = z.object({
  collegeName: z.string().min(1, "College name is required").max(200),
  applicationType: z.enum(["earlyDecision", "earlyAction", "regularDecision", "rolling"]),
  deadline: z.string().min(1, "Deadline is required"),
  status: z.enum(["notStarted", "inProgress", "submitted", "accepted", "rejected", "waitlisted"]),
  requirementsText: z.string().optional().or(z.literal("")),
});

export type CollegeApplicationFormValues = z.infer<typeof collegeApplicationFormSchema>;
