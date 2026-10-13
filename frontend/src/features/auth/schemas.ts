import { z } from "zod";

export const loginSchema = z.object({
  email: z.string().min(1, "Email is required").email("Enter a valid email address"),
  password: z.string().min(1, "Password is required"),
});

export type LoginFormValues = z.infer<typeof loginSchema>;

// Mirrors StudentStudyHub.Application.Auth.RegisterRequestValidator (FluentValidation) on the backend.
export const registerSchema = z
  .object({
    email: z.string().min(1, "Email is required").email("Enter a valid email address"),
    password: z
      .string()
      .min(8, "Password must be at least 8 characters long")
      .regex(/[A-Z]/, "Password must contain at least one uppercase letter")
      .regex(/[a-z]/, "Password must contain at least one lowercase letter")
      .regex(/[0-9]/, "Password must contain at least one digit"),
    confirmPassword: z.string().min(1, "Please confirm your password"),
    firstName: z.string().min(1, "First name is required").max(100),
    lastName: z.string().min(1, "Last name is required").max(100),
    gradeLevel: z.coerce.number().int().min(9, "Grade must be 9-12").max(12, "Grade must be 9-12"),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

export type RegisterFormInput = z.input<typeof registerSchema>;
export type RegisterFormValues = z.output<typeof registerSchema>;
