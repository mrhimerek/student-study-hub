import { z } from "zod";

// Mirrors StudentStudyHub.Application.Users.UpdateProfileRequestValidator (FluentValidation) on the backend.
export const profileSchema = z.object({
  firstName: z.string().min(1, "First name is required").max(100),
  lastName: z.string().min(1, "Last name is required").max(100),
  gradeLevel: z.coerce.number().int().min(9, "Grade must be 9-12").max(12, "Grade must be 9-12"),
  schoolName: z
    .string()
    .max(200)
    .optional()
    .transform((v) => (v && v.trim().length > 0 ? v.trim() : null)),
});

export type ProfileFormInput = z.input<typeof profileSchema>;
export type ProfileFormValues = z.output<typeof profileSchema>;
