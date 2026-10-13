import { z } from "zod";

function numericStringInRange(min: number, max: number, message: string) {
  return z
    .string()
    .min(1, message)
    .refine((v) => {
      const n = Number(v);
      return Number.isFinite(n) && n >= min && n <= max;
    }, message);
}

// Mirrors StudentStudyHub.Application.SatPlanning.CreateSatPlanRequestValidator /
// UpdateSatPlanRequestValidator on the backend.
export const satPlanFormSchema = z.object({
  targetTestDate: z.string().min(1, "Target test date is required"),
  goalScore: numericStringInRange(400, 1600, "Goal score must be between 400 and 1600"),
});

export type SatPlanFormValues = z.infer<typeof satPlanFormSchema>;

// Mirrors StudentStudyHub.Application.SatPlanning.LogSatPracticeScoreRequestValidator.
export const satPracticeScoreFormSchema = z.object({
  testDate: z.string().min(1, "Test date is required"),
  readingWritingScore: numericStringInRange(200, 800, "Must be between 200 and 800"),
  mathScore: numericStringInRange(200, 800, "Must be between 200 and 800"),
});

export type SatPracticeScoreFormValues = z.infer<typeof satPracticeScoreFormSchema>;
