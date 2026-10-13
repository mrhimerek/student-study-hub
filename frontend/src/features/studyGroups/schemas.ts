import { z } from "zod";

// Mirrors StudentStudyHub.Application.StudyGroups.CreateStudyGroupRequestValidator /
// UpdateStudyGroupRequestValidator on the backend.
export const studyGroupFormSchema = z.object({
  name: z.string().min(1, "Name is required").max(150),
  subject: z.string().max(100).optional().or(z.literal("")),
  schoolName: z.string().min(1, "School is required").max(200),
});

export type StudyGroupFormValues = z.infer<typeof studyGroupFormSchema>;

export const studyGroupEditFormSchema = z.object({
  name: z.string().min(1, "Name is required").max(150),
  subject: z.string().max(100).optional().or(z.literal("")),
});

export type StudyGroupEditFormValues = z.infer<typeof studyGroupEditFormSchema>;

export const joinStudyGroupFormSchema = z.object({
  inviteCode: z.string().min(1, "Invite code is required").max(8),
});

export type JoinStudyGroupFormValues = z.infer<typeof joinStudyGroupFormSchema>;

// Mirrors StudentStudyHub.Application.StudyGroups.CreateGroupNoteRequestValidator /
// UpdateGroupNoteRequestValidator on the backend.
export const groupNoteFormSchema = z.object({
  title: z.string().min(1, "Title is required").max(200),
  content: z.string().max(10000).optional().or(z.literal("")),
});

export type GroupNoteFormValues = z.infer<typeof groupNoteFormSchema>;
